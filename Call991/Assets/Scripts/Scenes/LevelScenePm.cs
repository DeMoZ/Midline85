using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Configs;
using Data;
using UI;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelScenePm : IDisposable
{
    public struct Ctx
    {
        public AudioManager audioManager;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public ReactiveCommand onClickMenuButton;
        public ReactiveCommand<PhraseEvent> onPhraseSoundEvent;
        public ReactiveCommand<PhraseSet> onShowPhrase;
        public ReactiveCommand<PhraseSet> onHidePhrase;
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
    }

    private Ctx _ctx;
    private CompositeDisposable _disposables;
    private PhraseSet _currentPhrase;
    private ReactiveCommand<int> _onClickChoiceButton;

    private bool _choiceDone;
    private float _phraseTimer;
    
    /// <summary>
    /// Choice button selection with keyboard.
    /// </summary>
    private bool _selectionPlaced;
    
    public LevelScenePm(Ctx ctx)
    {
        Debug.Log($"[{this}] constructor");

        _ctx = ctx;
        _disposables = new CompositeDisposable();

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

    private void SetPause(bool pause)
    {
        Time.timeScale = pause ? 0 : 1;
        _ctx.phraseSoundPlayer.Pause(pause);
        _ctx.audioManager.Pause(pause);
        
        if(!pause)
            _selectionPlaced = false;
    }

    private void OnSkipPhrase()
    {
        if (_currentPhrase != null && _currentPhrase.nextIs != NextIs.LevelEnd)
            _phraseTimer = _currentPhrase.Phrase.Duration(_currentPhrase.textAppear);
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

        await _ctx.phraseEventVideoLoader.LoadVideoEvent(_ctx.chapterSet.titleVideoSoName);
        await ShowNewsPaper();
        await ShowIntro();
        await _ctx.phraseEventVideoLoader.LoadVideoEvent(_ctx.chapterSet.levelVideoSoName);
        
        RunDialogue();
    }

    private async Task ShowNewsPaper()
    {
        var container = new Container<Task>();
        _ctx.onShowNewspaper.Execute((container, _ctx.newspaperSprite));
        await container.Value;
    }

    private async Task ShowIntro()
    {
        await _ctx.phraseEventVideoLoader.LoadVideoEvent(_ctx.chapterSet.titleVideoSoName);
        _ctx.onShowIntro.Execute(true);
        await Task.Delay((int) (_ctx.gameSet.levelIntroDelay * 1000));
        _ctx.onShowIntro.Execute(false);
    }

    private async Task RunDialogue()
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

            if (_currentPhrase.hidePhraseOnEnd || _currentPhrase.hidePersonOnEnd)
                _ctx.onHidePhrase.Execute(_currentPhrase); // TODO can be awaitable hide

            switch (_currentPhrase.nextIs)
            {
                case NextIs.Phrase:
                    NextPhrase();
                    break;
                case NextIs.Choices:
                    NextChoices();
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
        
        var rndChoice = choices[Random.Range(0,choices.Count)];

        if (IsBlocked(rndChoice))
        {
            choices.Remove(rndChoice);
            return RandomSelectButton(new List<Choice>(choices));
        }

        return rndChoice;
    }

    private async Task NextPhrase()
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
        _ctx.onShowPhrase.Execute(_currentPhrase);
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
                _ctx.phraseEventSoundLoader.LoadMusicEvent(pEvent.eventId);
                break;
            case PhraseEventTypes.Video:
                break;
            case PhraseEventTypes.Sfx:
                _ctx.onPhraseSoundEvent.Execute(pEvent); // TODO: is it required?
                _ctx.phraseEventSoundLoader.LoadSfxEvent(pEvent.eventId, false, pEvent.stop);
                break;
            case PhraseEventTypes.LoopSfx:
                _ctx.onPhraseSoundEvent.Execute(pEvent); // TODO: is it required?
                _ctx.phraseEventSoundLoader.LoadSfxEvent(pEvent.eventId, true, pEvent.stop);
                break;
            case PhraseEventTypes.Vfx:

                break;
            case PhraseEventTypes.LoopVfx:
                Debug.LogWarning($"[{this}] PhraseEventTypes.VideoLoop to be execute");
                _ctx.phraseEventVideoLoader.LoadVideoEvent(pEvent.eventId);
                break;
            case PhraseEventTypes.LevelEnd:
                Debug.LogWarning($"[{this}] PhraseEventTypes.LevelEnd to be execute");
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
        GameObjectEventSystemSelectionExtension.StopSelection();
        _ctx.profile.AddChoice(_currentPhrase.choices[index].choiceId);
        _ctx.profile.LastPhrase = _currentPhrase.choices[index].nextPhraseId;

        for (var i = 0; i < _ctx.buttons.Count; i++)
        {
            if (i == index)
            {
                _ctx.buttons[i].SetClicked();
                Debug.Log($"[{this}] pressed button {i}");
            }

            _ctx.buttons[i].Hide(i == index);
        }

        await Task.Delay((int) (_ctx.gameSet.slowButtonFadeDuration * 1000));

        foreach (var button in _ctx.buttons)
            button.gameObject.SetActive(false);

        RunDialogue();
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