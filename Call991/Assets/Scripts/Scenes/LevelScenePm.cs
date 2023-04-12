using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AaDialogueGraph;
using Configs;
using Core;
using Data;
using UI;
using UniRx;
using UnityEngine;
using ManualResetEvent = System.Threading.ManualResetEvent;
using Random = UnityEngine.Random;

public class LevelScenePm : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<List<AaNodeData>> FindNext;
        public ReactiveCommand<List<AaNodeData>> OnNext;

        public ReactiveCommand<UiPhraseData> OnShowPhrase;

        public AudioManager audioManager;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public ReactiveCommand onClickMenuButton;
        public ReactiveCommand<PhraseEvent> onPhraseSoundEvent;

        public ReactiveCommand<UiPhraseData> onHidePhrase;
        public ReactiveCommand<bool> onShowIntro;
        public ReactiveCommand<string> onPhraseLevelEndEvent;

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
    private ReactiveCommand<int> _onClickChoiceButton;

    private bool _choiceDone;
    private float _phraseTimer;
    private readonly ReactiveProperty<bool> _isPhraseSkipped = new ();

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

        _onClickChoiceButton = new ReactiveCommand<int>().AddTo(_disposables);
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
        Debug.LogError($"[{this}] OnSkipPhrase");
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
        Debug.LogError($"_ctx.FindNext.Execute with no data");
        _ctx.FindNext.Execute(new List<AaNodeData>());
    }
    
    // private CancellationToken _token;
    private List<Task> _tasks = new();

    private List<AaNodeData> _next;
    private List<ChoiceNodeData> _choices;

    private void OnDialogue(List<AaNodeData> data)
    {
        _isPhraseSkipped.Value = false;

        Debug.LogError($"Received new nodes {data.Count}");

        _next = new List<AaNodeData>();
        _choices = new List<ChoiceNodeData>();

        var phrases = data.OfType<PhraseNodeData>().ToList();
        var choices = data.OfType<ChoiceNodeData>().ToList();
        var ends = data.OfType<EndNodeData>().ToList();

        _next.AddRange(phrases);
        // _next.Add(choices); //if only selected by click or timeout
        // shoudl be level end on ends

        var observables = new IObservable<Unit>[] { };

        foreach (var phrase in phrases)
        {
            var routine = Observable.FromCoroutine(() => RunPhrase(phrase));
            observables = observables.Concat(new[] { routine }).ToArray();
        }
        
        if (choices.Any())
        {
            var routine = Observable.FromCoroutine(() => RunChoices(choices));
            observables = observables.Concat(new[] { routine }).ToArray();

            //_tasks.Add(choicesTask);
        }

        Observable.WhenAll(observables)
            .Subscribe(_ =>
            {
                Debug.Log("All coroutines completed");
                _next.AddRange(phrases);
                //_next.Add(choice);
                if (_next.Any())
                {
                    _ctx.FindNext?.Execute(_next);
                }
            }).AddTo(_disposables);


       

        // foreach (var end in ends)
        // {
        //     _tasks.Add(RunEnd(end));
        // }

        // await for showing, strike all events


        // ask for new
        // if (_choices.Any() && _notChosen)
        // {
        //     AutoselectChoice();
        //         // chosen will be added into _next
        // }

        // Debug.LogError($"[{this}] после ожидания тасок");
        // if (_next.Any())
        // {
        //     _ctx.FindNext?.Execute(_next);
        // }
    }

    private IEnumerator RunPhrase(PhraseNodeData data)
    {
        Debug.LogError($"RunPhrase {data}");
        float defaultTime = 4f;
        Phrase phrase = null;
        // get language
        // await try load dialogue for the language
        //     await try load dialogue for default language

        //await _ctx.phraseSoundPlayer.TryLoadDialogue(phraseByLanguage, _token);

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

        foreach (var t in Timer(time, _isPhraseSkipped, HideText )) yield return t;
        
        HideText();
        
        void HideText()
        {
            _ctx.onHidePhrase.Execute(uiPhrase);
        }
    }

    private IEnumerable Timer(float time, ReactiveProperty<bool> onSkip, Action onEnd )
    {
        var timer = 0f;
        
        while (timer <= time)
        {
            yield return null;
            timer += Time.deltaTime;

            if (onSkip.Value)
            {
                onEnd?.Invoke();
                yield break;
            }
        }
    }

    private IEnumerator RunChoices(List<ChoiceNodeData> data)
    {
        // todo yield for timer and expect click  
        // on click yield for hideButtons
        yield return null;
    }
    /*private async Task RunChoices(List<ChoiceNodeData> data)
    {
        Debug.LogError($"RunChoices {data.Count}");

        GameObjectEventSystemSelectionExtension.ClearSelection();

        foreach (var choice in data)
        {
            _ctx.buttons[_choices.Count].interactable = !choice.IsLocked;
            _ctx.buttons[_choices.Count].Show(choice.Choice, choice.IsLocked);
        }

        Debug.LogError("the rest of the method is not implemented yet");

        var resetEvent = new ManualResetEvent(false);
        Observable.FromCoroutine(ct => ChoiceRoutine(ct, resetEvent))
            .Subscribe(_ =>
            {
                Debug.Log($"[{this}] Choice coroutine end");
                // Вызываем метод Set(), чтобы разблокировать поток
                resetEvent.Set();
            })
            .AddTo(_disposables);

        //await resetEvent.WaitOne();

        // Observable.FromCoroutine(ChoiceRoutine).Subscribe(_ =>
        //     {
        //         Debug.Log($"[{this}] Choice coroutine end");
        //     })
        //     .AddTo(_disposables);

        //await Task.Delay((int)(_ctx.gameSet.choicesDuration * 1000), _token);
    }
    */

    private void RunEndNode(EndNodeData data)
    {
        Debug.LogError($"EndNodeData {data}");

        throw new NotImplementedException();
    }

    //------------------------------------------------------------------------------------------------------------------

    private async void RunDialogue()
    {
        _currentPhrase = _ctx.dialogues.phrases.FirstOrDefault(p => p.phraseId == _ctx.profile.LastPhrase);
        if (_currentPhrase == null)
        {
            Debug.LogError($"[{this}] _ctx.profile.LastPhrase: {_ctx.profile.LastPhrase}. Not found in phrases.");
            return;
        }

        await _ctx.phraseSoundPlayer.TryLoadDialogue(_currentPhrase.Phrase.GetOverridenPhraseId());

        Observable.FromCoroutine(PhraseRoutine).Subscribe(_ =>
        {
            Debug.Log($"[{this}] Coroutine end");

            // if (_currentPhrase.hidePhraseOnEnd || _currentPhrase.hidePersonOnEnd)
            //     _ctx.onHidePhrase.Execute(_currentPhrase); // TODO can be awaitable hide

            switch (_currentPhrase.nextIs)
            {
                case NextIs.Phrase:
                    NextPhrase();
                    break;
                case NextIs.Choices:
                    //NextChoices();
                    break;
                case NextIs.LevelEnd:
                    Debug.LogWarning($"[{this}] LevelEnd on {_currentPhrase.phraseId}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }).AddTo(_disposables);
    }

    private void NextChoices()
    {
        if (_currentPhrase.choices.Count == 0)
        {
            Debug.LogWarning($"[{this}] no choices set up for phrase {_currentPhrase.phraseId}");
            return;
        }

        _choiceDone = false;

        GameObjectEventSystemSelectionExtension.ClearSelection();

        for (int i = 0; i < _currentPhrase.choices.Count; i++)
        {
            if (_currentPhrase.choices[i].ifSelected)
            {
                Debug.Log($" {_currentPhrase.phraseId} choices {_currentPhrase.choices[i].choiceId} has requirements");
                PrintArray(_currentPhrase.choices[i].requiredChoices);
            }

            var isBlocked = IsBlocked(_currentPhrase.choices[i]);
            _ctx.buttons[i].interactable = !isBlocked;
            _ctx.buttons[i].Show(_currentPhrase.choices[i].choiceId, isBlocked);
        }

        PrintArray(_ctx.profile.GetPlayerData().choices);

        Observable.FromCoroutine(ChoiceRoutine).Subscribe(_ => { Debug.Log($"[{this}] Choice coroutine end"); })
            .AddTo(_disposables);
    }

    private bool IsBlocked(Choice choice)
    {
        if (choice.ifSelected)
            return !_ctx.profile.ContainsChoice(choice.requiredChoices);

        return false;
    }

    private IEnumerator ChoiceRoutine(CancellationToken ct, ManualResetEvent resetEvent)
    {
        while (!ct.IsCancellationRequested)
        {
            yield return new WaitForSeconds(1);
            Debug.Log($"[{this}] Choice coroutine running...");
        }

        // Посылаем событие окончания работы
        resetEvent.Set();

        // Останавливаем корутину
        yield break;
    }

    private IEnumerator ChoiceRoutine()
    {
        var timer = 0f;

        var time = _currentPhrase.overrideChoicesDuration
            ? _currentPhrase.choicesDuration
            : _ctx.gameSet.choicesDuration;

        _ctx.countDown.Show(time);
        _ctx.audioManager.PlayUiSound(SoundUiTypes.Timer, true);

        yield return new WaitForSeconds(_ctx.gameSet.buttonsAppearDuration);

        _selectionPlaced = false;

        while (timer <= time)
        {
            yield return null;
            timer += Time.deltaTime;

            if (_choiceDone)
                yield break;

            CheckForSelectionPlaced();
        }

        Debug.LogWarning($"[{this}] choice time up! Random choice!");

        var isBlocked = true;
        var index = 0;
        while (isBlocked)
        {
            index = Random.Range(0, _currentPhrase.choices.Count);
            isBlocked = IsBlocked(_currentPhrase.choices[index]);
            if (isBlocked)
                yield return null;
        }

        _ctx.buttons[index].gameObject.Select();
        _ctx.buttons[index].Press();
        OnClickChoiceButton(index);
    }

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

    private void NextPhrase()
    {
        if (string.IsNullOrWhiteSpace(_currentPhrase.nextId))
        {
            Debug.LogWarning($"[{this}] nextId not set up for phrase {_currentPhrase.phraseId}");
            return;
        }

        _ctx.profile.LastPhrase = _currentPhrase.nextId;
        RunDialogue();
    }

    private IEnumerator PhraseRoutine()
    {
        if (_currentPhrase == null)
        {
            Debug.LogError($"[{this}] No phrase found for id {_ctx.profile.LastPhrase}");
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
                Debug.LogWarning($"[{this}] PhraseEventTypes.VideoLoop to be execute");
                _ctx.phraseEventVideoLoader.LoadVideoEvent(pEvent.eventId).Forget();
                break;
            case PhraseEventTypes.LevelEnd:
                Debug.LogWarning($"[{this}] PhraseEventTypes.LevelEnd to be execute");
                // _ctx.cursorSettings.EnableCursor(false);
                _ctx.onPhraseLevelEndEvent.Execute(pEvent.eventId);
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

        for (int i = 0; i < _ctx.buttons.Count; i++)
        {
            _ctx.buttons[i].SetCtx(new ChoiceButtonView.Ctx
            {
                index = i,
                onClickChoiceButton = _onClickChoiceButton,
                buttonsAppearDuration = _ctx.gameSet.buttonsAppearDuration,
                fastButtonFadeDuration = _ctx.gameSet.fastButtonFadeDuration,
                slowButtonFadeDuration = _ctx.gameSet.slowButtonFadeDuration,
            });
        }
    }

    private async void OnClickChoiceButton(int index)
    {
        if (_choiceDone) return;

        _ctx.audioManager.StopTimer();
        _ctx.audioManager.PlayUiSound(SoundUiTypes.ChoiceButton);

        _ctx.countDown.Stop(_ctx.gameSet.fastButtonFadeDuration);
        _choiceDone = true;
        //GameObjectEventSystemSelectionExtension.StopSelection();
        _ctx.profile.AddChoice(_currentPhrase.choices[index].choiceId);
        _ctx.profile.LastPhrase = _currentPhrase.choices[index].nextPhraseId;

        await Task.Delay((int)(_ctx.gameSet.slowButtonFadeDuration * 1000));

        foreach (var button in _ctx.buttons)
            button?.gameObject?.SetActive(false);

        RunDialogue();
    }

    public void Dispose()
    {
        //_token.ThrowIfCancellationRequested();
        _disposables.Dispose();
    }

    private static void PrintArray(List<string> array)
    {
        var s = array.Aggregate("", (current, t) => current + ", " + t);
        Debug.Log(s);
    }
}