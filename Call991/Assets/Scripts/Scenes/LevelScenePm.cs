using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AaDialogueGraph;
using Configs;
using Core;
using Data;
using UI;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelScenePm : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<List<AaNodeData>> FindNext;
        public ReactiveCommand<List<AaNodeData>> OnNext;
        public ReactiveCommand<UiPhraseData> OnShowPhrase;
        public List<string> Languages;

        public AudioManager audioManager;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public ReactiveCommand onClickMenuButton;
        public ReactiveCommand<PhraseEvent> onPhraseSoundEvent;

        public ReactiveCommand<UiPhraseData> onHidePhrase;
        public ReactiveCommand<bool> onShowIntro;
        public ReactiveCommand<List<RecordData>> OnLevelEnd;

        public Dialogues dialogues;
        public PlayerProfile profile;
        public List<ChoiceButtonView> buttons;
        public CountDownView countDown;

        public ReactiveCommand onAfterEnter;
        public GameSet gameSet;

        public PhraseSoundPlayer phraseSoundPlayer;
        public PhraseEventSoundLoader phraseEventSoundLoader;
        public Sprite newspaperSprite;
        public ReactiveCommand<(Container<Task> task, Sprite sprite)> onShowNewspaper;
        public ChapterSet chapterSet;
        public PhraseEventVideoLoader phraseEventVideoLoader;
        public ReactiveCommand onSkipPhrase;
        public ReactiveCommand<bool> onClickPauseButton;
        public VideoManager videoManager;
        public Blocker blocker;
        public CursorSet cursorSettings;
    }

    private Ctx _ctx;
    private CompositeDisposable _disposables;
    private PhraseSet _currentPhrase;
    private ReactiveCommand<ChoiceButtonView> _onClickChoiceButton;

    private bool _choiceDone;
    private float _phraseTimer;
    private readonly ReactiveProperty<bool> _isPhraseSkipped = new();
    private readonly ReactiveProperty<bool> _isChoiceDone = new();

    /// <summary>
    /// Choice button selection with keyboard.
    /// </summary>
    private bool _selectionPlaced;

    public LevelScenePm(Ctx ctx)
    {
        Debug.Log($"[{this}] constructor");

        _ctx = ctx;
        _disposables = new CompositeDisposable();

        _ctx.OnNext.Subscribe(OnDialogue).AddTo(_disposables);

        _onClickChoiceButton = new ReactiveCommand<ChoiceButtonView>().AddTo(_disposables);
        _onClickChoiceButton.Subscribe(OnClickChoiceButton).AddTo(_disposables);
        _ctx.onSkipPhrase.Subscribe(_ => OnSkipPhrase()).AddTo(_disposables);

        _ctx.onClickPauseButton.Subscribe(SetPause).AddTo(_disposables);

        _ctx.onClickMenuButton.Subscribe(_ =>
        {
            _ctx.audioManager.PlayUiSound(SoundUiTypes.MenuButton);
            _ctx.onSwitchScene.Execute(GameScenes.Menu);
        }).AddTo(_disposables);
        _ctx.onAfterEnter.Subscribe(_ => OnAfterEnter()).AddTo(_disposables);
    }

    private async void OnAfterEnter()
    {
        InitButtons();

#if !BUILD_PRODUCTION
        // todo refactoring to support both replay level in editor and build
        if (!string.IsNullOrWhiteSpace(_ctx.profile.CheatPhrase))
        {
            _ctx.profile.ClearPhrases();
            _ctx.profile.ClearChoices();

            _ctx.profile.LastPhrase = _ctx.profile.CheatPhrase;
        }
#endif

        if (string.IsNullOrWhiteSpace(_ctx.profile.LastPhrase))
            _ctx.profile.LastPhrase = _ctx.dialogues.phrases[0].phraseId;
        //TODO to not bother me
        // await ShowNewsPaper();
        // await Task.Delay(500);
        await ShowIntro();
        await _ctx.phraseEventVideoLoader.LoadVideoSoToPrepareVideo(_ctx.chapterSet.levelVideoSoName);
        _ctx.videoManager.PlayPreparedVideo();
        await Task.Delay(500);
        ExecuteDialogue();
        await _ctx.blocker.FadeScreenBlocker(false);
        _ctx.cursorSettings.EnableCursor(true);
    }

    private void SetPause(bool pause)
    {
        Time.timeScale = pause ? 0 : 1;
        _ctx.phraseSoundPlayer.Pause(pause);
        _ctx.audioManager.Pause(pause);

        if (!pause)
            _selectionPlaced = false;
    }

    private void OnSkipPhrase()
    {
        Debug.Log($"[{this}] OnSkipPhrase");
        _isPhraseSkipped.Value = true;
    }

    private async Task ShowNewsPaper()
    {
        await _ctx.blocker.FadeScreenBlocker(true);

        var container = new Container<Task>();
        _ctx.onShowNewspaper.Execute((container, _ctx.newspaperSprite));

        await _ctx.blocker.FadeScreenBlocker(false);
        _ctx.cursorSettings.EnableCursor(true);

        await container.Value;
        _ctx.cursorSettings.EnableCursor(false);
    }

    [Obsolete]
    private async Task ShowIntro()
    {
        // _ctx.blocker.EnableScreenFade(true);
        // await _ctx.phraseEventVideoLoader.LoadVideoSoToPrepareVideo(_ctx.chapterSet.titleVideoSoName);
        // _ctx.videoManager.PlayPreparedVideo();
        // _ctx.onShowIntro.Execute(true);
        // await _ctx.blocker.FadeScreenBlocker(false);
        // await Task.Delay((int)(_ctx.gameSet.levelIntroDelay * 1000));
        await _ctx.blocker.FadeScreenBlocker(true);
        _ctx.onShowIntro.Execute(false);
    }

    private void ExecuteDialogue()
    {
        Debug.Log($"[{this}] _ctx.FindNext.Execute with no data");
        _ctx.FindNext.Execute(new List<AaNodeData>());
    }

    private List<AaNodeData> _next;
    private List<ChoiceNodeData> _choices;
    private ChoiceNodeData _choice;

    private void OnDialogue(List<AaNodeData> data)
    {
        _isPhraseSkipped.Value = false;
        _isChoiceDone.Value = false;

        Debug.Log($"[{this}] Received new nodes {data.Count}");

        _next = new List<AaNodeData>();
        _choice = null;

        var phrases = data.OfType<PhraseNodeData>().ToList();
        _choices = data.OfType<ChoiceNodeData>().ToList();

        var ends = data.OfType<EndNodeData>().ToList();
        if (ends.Any())
        {
            RunEndNode(ends);
            return;
//------------------
        }

        var observables = new IObservable<Unit>[] { };

        foreach (var phrase in phrases)
        {
            var routine = Observable.FromCoroutine(() => RunPhrase(phrase));
            observables = observables.Concat(new[] { routine }).ToArray();
        }

        if (_choices.Any())
        {
            var routine = Observable.FromCoroutine(() => RunChoices(_choices));
            observables = observables.Concat(new[] { routine }).ToArray();
        }

        Observable.WhenAll(observables)
            .Subscribe(_ =>
            {
                Debug.Log($"[{this}] All coroutines completed");
                _next.AddRange(phrases);
                _next.Add(_choice);
                if (_next.Any())
                {
                    _ctx.FindNext?.Execute(_next);
                }
            }).AddTo(_disposables);

        // TODO await for showing, strike all events
    }

    private IEnumerator RunPhrase(PhraseNodeData data)
    {
        Debug.Log($"[{this}] RunPhrase {data}");
        var defaultTime = 4f;

        var phrase = GetPhrase(data);
        var audioClip = GetVoice(data);

        var uiPhrase = new UiPhraseData
        {
            DefaultTime = defaultTime,
            Description = data.PhraseSketchText,
            PersonVisualData = data.PersonVisualData,
            PhraseVisualData = data.PhraseVisualData,
            Phrase = phrase,
        };

        _ctx.OnShowPhrase.Execute(uiPhrase);

        var time = phrase == null ? defaultTime : phrase.totalTime;
        foreach (var t in Timer(time, _isPhraseSkipped, HideText)) yield return t;

        HideText();

        void HideText()
        {
            _ctx.onHidePhrase.Execute(uiPhrase);
        }
    }

    private Phrase GetPhrase(PhraseNodeData data)
    {
        if (_ctx.Languages == null || _ctx.Languages.Count == 0) return null;

        var index = _ctx.Languages.IndexOf(_ctx.profile.TextLanguage.ToString());

        if (index == -1) return null;

        Phrase result = null;

        // TODO this loading should be awaitable and asynchronous
        result = NodeUtils.GetObjectByPath<Phrase>(data.Phrases[index])
                 ?? NodeUtils.GetObjectByPath<Phrase>(data.Phrases[0]);

        return result;
    }

    private AudioClip GetVoice(PhraseNodeData data)
    {
        if (_ctx.Languages == null || _ctx.Languages.Count == 0) return null;

        var index = _ctx.Languages.IndexOf(_ctx.profile.AudioLanguage.ToString());

        if (index == -1) return null;

        AudioClip result = null;

        // TODO this loading should be awaitable and asynchronous
        result = NodeUtils.GetObjectByPath<AudioClip>(data.PhraseSounds[index])
                 ?? NodeUtils.GetObjectByPath<AudioClip>(data.PhraseSounds[0]);

        //await _ctx.phraseSoundPlayer.TryLoadDialogue(phraseByLanguage);
        return result;
    }

    private IEnumerator RunChoices(List<ChoiceNodeData> data)
    {
        for (var i = 0; i < data.Count; i++)
        {
            var choice = data[i];
            _ctx.buttons[i].interactable = !choice.IsLocked;
            _ctx.buttons[i].Show(choice.Choice, choice.IsLocked);
        }

        foreach (var t in Timer(_ctx.gameSet.choicesDuration, _isChoiceDone)) yield return t;

        if (!_isChoiceDone.Value)
        {
            AutoChoice(data);
        }

        _ctx.audioManager.StopTimer();
        _ctx.audioManager.PlayUiSound(SoundUiTypes.ChoiceButton);
        _ctx.countDown.Stop(_ctx.gameSet.fastButtonFadeDuration);

        foreach (var t in Timer(_ctx.gameSet.slowButtonFadeDuration)) yield return t;

        foreach (var button in _ctx.buttons)
        {
            button.gameObject.SetActive(false);
        }
    }

    private void AutoChoice(List<ChoiceNodeData> data)
    {
        Debug.Log($"[{this}] <color=yellow>choice time up!</color> Random choice!");

        var cnt = data.Count;
        var isBlocked = true;
        var index = 0;
        while (isBlocked)
        {
            index = Random.Range(0, cnt);
            isBlocked = data[index].IsLocked;
        }

        _ctx.buttons[index].gameObject.Select();
        _ctx.buttons[index].Press();
        OnClickChoiceButton(_ctx.buttons[index]);
    }

    private void OnClickChoiceButton(ChoiceButtonView buttonView)
    {
        if (_isChoiceDone.Value) return;
        _isChoiceDone.Value = true;

        _choice = _choices[_ctx.buttons.IndexOf(buttonView)];
    }

    private async void RunEndNode(List<EndNodeData> data)
    {
        Debug.Log($"[{this}] level end {data.Count}");

        // TODO move into event on Phrase Node
        //_ctx.onHideLevelUi.Execute(_ctx.gameSet.levelEndLevelUiDisappearTime);
        //await Task.Delay((int) (_ctx.gameSet.levelEndLevelUiDisappearTime * 1000));
        //Debug.LogWarning($"[{this}] on hide awaited");Ë

        _ctx.OnLevelEnd.Execute(data.First().Records);

        // TODO fade should be removed and implemented with prefab in envent field
        await Task.Delay((int)(_ctx.gameSet.levelEndStatisticsUiFadeTime * 1000));
    }

    private IEnumerator ObserveTimer(float time, ReactiveProperty<bool> onSkip = null, Action onEnd = null)
    {
        foreach (var t in Timer(time, onSkip, onEnd)) yield return t;
    }


    private IEnumerable Timer(float time, ReactiveProperty<bool> onSkip = null, Action onEnd = null)
    {
        var timer = 0f;

        while (timer <= time)
        {
            yield return null;
            timer += Time.deltaTime;

            if (onSkip is { Value: true })
            {
                onEnd?.Invoke();
                yield break;
            }
        }
    }
//------------------------------------------------------------------------------------------------------------------

// private bool IsBlocked(Choice choice)
// {
//     if (choice.ifSelected)
//         return !_ctx.profile.ContainsChoice(choice.requiredChoices);
//
//     return false;
// }

/*TODO This might be needed
 private void CheckForSelectionPlaced()
{
    if (!_selectionPlaced && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                              Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)))
    {
        // set first selection
        var eventSystemSelectGameObject = RandomSelectButton(new List<Choice>(_currentPhrase.choices));

        if (eventSystemSelectGameObject != null)
        {
            var selectIndex = _currentPhrase.choices.IndexOf(eventSystemSelectGameObject);
            _ctx.buttons[selectIndex].gameObject.Select();
        }

        _selectionPlaced = true;
    }
}

private Choice RandomSelectButton(List<Choice> choices)
{
    if (choices.Count == 0)
        return null;

    var rndChoice = choices[Random.Range(0, choices.Count)];

    if (IsBlocked(rndChoice))
    {
        choices.Remove(rndChoice);
        return RandomSelectButton(new List<Choice>(choices));
    }

    return rndChoice;
}
*/

    private IEnumerator PhraseRoutine()
    {
        if (_currentPhrase == null)
        {
            Debug.Log($"[{this}] No phrase found for id {_ctx.profile.LastPhrase}");
            yield break;
        }

        _phraseTimer = 0f;

        var pEvents = new List<PhraseEvent>();
        if (_currentPhrase.addEvent)
            pEvents.AddRange(_currentPhrase.phraseEvents);

        Debug.Log($"[{this}] Execute event for phrase {_currentPhrase.phraseId}: {_currentPhrase.Phrase.text}");
        //_ctx.onShowPhrase.Execute(_currentPhrase);
        _ctx.phraseSoundPlayer.TryPlayPhraseFile();

        while (_phraseTimer <= _currentPhrase.Phrase.Duration(_currentPhrase.textAppear))
        {
            yield return null;

            for (var i = pEvents.Count - 1; i >= 0; i--)
            {
                var pEvent = pEvents[i];
                if (_phraseTimer >= pEvent.delay)
                {
                    ExecutePhraseEvent(pEvent);
                    pEvents.RemoveAt(i);
                }
            }

            _phraseTimer += Time.deltaTime;
        }

        // of somehow an event was skipped, strike it
        foreach (var pEvent in pEvents)
            ExecutePhraseEvent(pEvent);
    }

    private void ExecutePhraseEvent(PhraseEvent pEvent)
    {
        switch (pEvent.eventType)
        {
            case PhraseEventTypes.Music:
                _ctx.phraseEventSoundLoader.LoadMusicEvent(pEvent.eventId).Forget();
                ;
                break;
            case PhraseEventTypes.Video:
                break;
            case PhraseEventTypes.Sfx:
                _ctx.onPhraseSoundEvent.Execute(pEvent); // TODO: is it required?
                _ctx.phraseEventSoundLoader.LoadSfxEvent(pEvent.eventId, false, pEvent.stop).Forget();
                ;
                break;
            case PhraseEventTypes.LoopSfx:
                _ctx.onPhraseSoundEvent.Execute(pEvent); // TODO: is it required?
                _ctx.phraseEventSoundLoader.LoadSfxEvent(pEvent.eventId, true, pEvent.stop).Forget();
                ;
                break;
            case PhraseEventTypes.Vfx:

                break;
            case PhraseEventTypes.LoopVfx:
                Debug.Log($"[{this}] PhraseEventTypes.VideoLoop to be execute");
                _ctx.phraseEventVideoLoader.LoadVideoEvent(pEvent.eventId).Forget();
                break;
            case PhraseEventTypes.LevelEnd:
                Debug.Log($"[{this}] PhraseEventTypes.LevelEnd to be execute");
                // _ctx.cursorSettings.EnableCursor(false);
                //_ctx.OnLevelEnd.Execute(pEvent.eventId);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void InitButtons()
    {
        _ctx.countDown.SetCtx(new CountDownView.Ctx
        {
            buttonsAppearDuration = _ctx.gameSet.buttonsAppearDuration,
        });

        foreach (var button in _ctx.buttons)
        {
            button.SetCtx(new ChoiceButtonView.Ctx
            {
                onClickChoiceButton = _onClickChoiceButton,
                buttonsAppearDuration = _ctx.gameSet.buttonsAppearDuration,
                fastButtonFadeDuration = _ctx.gameSet.fastButtonFadeDuration,
                slowButtonFadeDuration = _ctx.gameSet.slowButtonFadeDuration,
            });
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    private static void PrintArray(List<string> array)
    {
        var s = array.Aggregate("", (current, t) => current + ", " + t);
        Debug.Log(s);
    }
}