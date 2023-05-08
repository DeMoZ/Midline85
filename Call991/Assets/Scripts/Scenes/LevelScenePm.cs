using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AaDialogueGraph;
using Configs;
using Data;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class LevelScenePm : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<List<AaNodeData>> FindNext;
        public ReactiveCommand<List<AaNodeData>> OnNext;
        public ReactiveCommand<UiPhraseData> OnShowPhrase;
        public PlayerProfile Profile;

        public AudioManager AudioManager;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public ReactiveCommand onClickMenuButton;
        public ReactiveCommand<UiPhraseData> onHidePhrase;
        public ReactiveCommand<bool> onShowIntro;
        public ReactiveCommand<List<RecordData>> OnLevelEnd;
        public ObjectEvents ObjectEvents;

        public PhraseSoundPlayer PhraseSoundPlayer;
        public ContentLoader ContentLoader;
        public List<ChoiceButtonView> buttons;
        public CountDownView countDown;

        public ReactiveCommand onAfterEnter;
        public GameSet gameSet;

        public Sprite newspaperSprite;
        public ReactiveCommand<(Container<Task> task, Sprite sprite)> onShowNewspaper;
        public ChapterSet chapterSet;
        
        public ReactiveCommand onSkipPhrase;
        public ReactiveCommand<bool> onClickPauseButton;
        public VideoManager videoManager;
        public Blocker Blocker;
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
    public CancellationTokenSource _tokenSource;

    /// <summary>
    /// Choice button selection with keyboard.
    /// </summary>
    private bool _selectionPlaced;

    public LevelScenePm(Ctx ctx)
    {
        Debug.Log($"[{this}] constructor");

        _ctx = ctx;
        _disposables = new CompositeDisposable();
        _tokenSource = new CancellationTokenSource().AddTo(_disposables);

        _ctx.OnNext.Subscribe(OnDialogue).AddTo(_disposables);

        _onClickChoiceButton = new ReactiveCommand<ChoiceButtonView>().AddTo(_disposables);
        _onClickChoiceButton.Subscribe(OnClickChoiceButton).AddTo(_disposables);
        _ctx.onSkipPhrase.Subscribe(_ => OnSkipPhrase()).AddTo(_disposables);

        _ctx.onClickPauseButton.Subscribe(SetPause).AddTo(_disposables);

        _ctx.onClickMenuButton.Subscribe(_ =>
        {
            _ctx.AudioManager.PlayUiSound(SoundUiTypes.MenuButton);
            _ctx.onSwitchScene.Execute(GameScenes.Menu);
        }).AddTo(_disposables);
        _ctx.onAfterEnter.Subscribe(_ => OnAfterEnter()).AddTo(_disposables);
    }

    private async void OnAfterEnter()
    {
        InitButtons();

#if !BUILD_PRODUCTION
        // todo refactoring to support both replay level in editor and build
        if (!string.IsNullOrWhiteSpace(_ctx.Profile.CheatPhrase))
        {
            _ctx.Profile.ClearPhrases();
            _ctx.Profile.ClearChoices();

            _ctx.Profile.LastPhrase = _ctx.Profile.CheatPhrase;
        }
#endif

        // if (string.IsNullOrWhiteSpace(_ctx.Profile.LastPhrase))
        //     _ctx.Profile.LastPhrase = _ctx.dialogues.phrases[0].phraseId;

        //TODO to not bother me
        // await ShowNewsPaper();
        // await Task.Delay(500);
        await ShowIntro();
        
        //await _ctx.phraseEventVideoLoader.LoadVideoSoToPrepareVideo(_ctx.chapterSet.levelVideoSoName);
        
        _ctx.videoManager.PlayPreparedVideo();
        await Task.Delay(500);
        ExecuteDialogue();
        await _ctx.Blocker.FadeScreenBlocker(false);
        _ctx.cursorSettings.EnableCursor(true);
    }

    private void SetPause(bool pause)
    {
        Time.timeScale = pause ? 0 : 1;
        _ctx.PhraseSoundPlayer.Pause(pause);
        _ctx.AudioManager.Pause(pause);

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
        await _ctx.Blocker.FadeScreenBlocker(true);

        var container = new Container<Task>();
        _ctx.onShowNewspaper.Execute((container, _ctx.newspaperSprite));

        await _ctx.Blocker.FadeScreenBlocker(false);
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
        await _ctx.Blocker.FadeScreenBlocker(true);
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

    private async void OnDialogue(List<AaNodeData> data)
    {
        _isPhraseSkipped.Value = false;
        _isChoiceDone.Value = false;

        Debug.Log($"[{this}] Received new nodes {data.Count}");

        _next = new List<AaNodeData>();
        _choice = null;

        var phrases = data.OfType<PhraseNodeData>().ToList();
        var ends = data.OfType<EndNodeData>().ToList();
        _choices = data.OfType<ChoiceNodeData>().ToList();
        var observables = new IObservable<Unit>[] { };
        var dialogueEvents = GetEvents(phrases, ends);
        var content = new Dictionary<string, object>();

        // load content with cancellation token. Return on Cancel.
        if (await LoadContent(content, dialogueEvents, phrases)) return;

        if (dialogueEvents.Count > 0)
        {
            foreach (var dialogueEvent in dialogueEvents)
            {
                var routine = Observable.FromCoroutine(() => RunDialogueEvent(dialogueEvent, content));
                observables = observables.Concat(new[] { routine }).ToArray();
            }
        }

        if (ends.Any())
        {
            var end = ends.First();
            dialogueEvents.AddRange(end.EventVisualData);
            RunEndNode(end);
        }

        foreach (var phraseData in phrases)
        {
            var phrase = content[$"p_{phraseData.Guid}"] as Phrase;
            var audioClip = content[$"a_{phraseData.Guid}"] as AudioClip;

            _ctx.PhraseSoundPlayer.PlayPhrase(audioClip);

            var routine = Observable.FromCoroutine(() => RunPhrase(phraseData, phrase));
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

                ResourcesLoader.UnloadUnused(); // todo debatable
            }).AddTo(_disposables);
    }

    private List<EventVisualData> GetEvents(List<PhraseNodeData> phrases, List<EndNodeData> ends)
    {
        var result = new List<EventVisualData>();

        foreach (var phrase in phrases.Where(phrase => phrase.EventVisualData.Any()))
        {
            result.AddRange(phrase.EventVisualData);
        }

        foreach (var end in ends.Where(end => end.EventVisualData.Any()))
        {
            result.AddRange(end.EventVisualData);
        }

        return result;
    }

    private async Task<bool> LoadContent(Dictionary<string, object> content, List<EventVisualData> dialogueEvents,
        List<PhraseNodeData> phrases)
    {
        foreach (var eventContent in dialogueEvents)
        {
            switch (eventContent.Type)
            {
                case PhraseEventType.AudioClip:
                    var audio = await _ctx.ContentLoader.GetObjectAsync<AudioClip>(eventContent.PhraseEvent);
                    content[eventContent.PhraseEvent] = audio;
                    break;
                case PhraseEventType.VideoClip:
                    var video = await _ctx.ContentLoader.GetObjectAsync<VideoClip>(eventContent.PhraseEvent);
                    content[eventContent.PhraseEvent] = video;
                    break;
                case PhraseEventType.GameObject:
                    var go = await _ctx.ContentLoader.GetObjectAsync<GameObject>(eventContent.PhraseEvent);
                    content[eventContent.PhraseEvent] = go;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_tokenSource.IsCancellationRequested) return true;
        }

        foreach (var phraseData in phrases)
        {
            var phrase = await _ctx.ContentLoader.GetPhraseAsync(phraseData);
            var audioClip = await _ctx.ContentLoader.GetVoiceAsync(phraseData);

            content[$"p_{phraseData.Guid}"] = phrase;
            content[$"a_{phraseData.Guid}"] = audioClip;

            if (_tokenSource.IsCancellationRequested) return true;
        }

        return false;
    }

    private IEnumerator RunPhrase(PhraseNodeData data, Phrase phrase)
    {
        Debug.Log($"[{this}] RunPhrase {data}");
        var defaultTime = 4f;

        var uiPhrase = new UiPhraseData
        {
            Description = data.PhraseSketchText,
            PersonVisualData = data.PersonVisualData,
            PhraseVisualData = data.PhraseVisualData,
            Phrase = phrase,
        };

        _ctx.OnShowPhrase.Execute(uiPhrase);

        var time = phrase == null ? defaultTime : phrase.totalTime;
        foreach (var t in Timer(time, _isPhraseSkipped)) yield return t;
        
        //Debug.LogError($"[{this}] hide Text called anyway");
        _ctx.onHidePhrase.Execute(uiPhrase);
    }

    private IEnumerator RunDialogueEvent(EventVisualData data, Dictionary<string, object> content)
    {
        foreach (var t in Timer(data.Delay, _isPhraseSkipped)) yield return t;
        yield return ProcessEvent(data, content);
    }

    private IEnumerator ProcessEvent(EventVisualData data, Dictionary<string, object> content)
    {
        switch (data.Type)
        {
            case PhraseEventType.AudioClip:
                _ctx.AudioManager.PlayEventSound(data, content[data.PhraseEvent] as AudioClip);
                break;
            case PhraseEventType.VideoClip:
                _ctx.videoManager.PlayVideo(data, content[data.PhraseEvent] as VideoClip);
                break;
            case PhraseEventType.GameObject:
                var prefab = content[data.PhraseEvent] as GameObject;
                
                if (prefab == null) break;
                
                var eventObject = Object.Instantiate(prefab).GetComponent<PhraseObjectEvent>();
                eventObject.SetCtx(_ctx.ObjectEvents, _ctx.gameSet);
                
                yield return eventObject.AwaitInvoke();
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        yield return null;
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

        _ctx.AudioManager.StopTimer();
        _ctx.AudioManager.PlayUiSound(SoundUiTypes.ChoiceButton);
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

    private void RunEndNode(EndNodeData data)
    {
        Debug.Log($"[{this}] level end {data}");
        _ctx.OnLevelEnd?.Execute(data.Records);
    }

    private IEnumerator ObserveTimer(float time, ReactiveProperty<bool> onSkip = null, Action onEnd = null)
    {
        foreach (var t in Timer(time, onSkip, onEnd)) yield return t;
    }


    private IEnumerable Timer(float time, ReactiveProperty<bool> skipSignal = null, Action onSkip = null)
    {
        var timer = 0f;

        while (timer <= time)
        {
            yield return null;
            timer += Time.deltaTime;

            if (skipSignal is { Value: true })
            {
                onSkip?.Invoke();
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
        _tokenSource.Cancel();
        _disposables.Dispose();
    }

    private static void PrintArray(List<string> array)
    {
        var s = array.Aggregate("", (current, t) => current + ", " + t);
        Debug.Log(s);
    }
}