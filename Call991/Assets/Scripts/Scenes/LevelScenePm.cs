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
        public ReactiveCommand<GameScenes> onSwitchScene;
        public ReactiveCommand onClickMenuButton;
        public ReactiveCommand<string> onPhraseEvent;
        public ReactiveCommand<Phrase> onShowPhrase;
        public ReactiveCommand<Phrase> onHidePhrase;

        public Dialogues dialogues;
        public PlayerProfile profile;
        public List<ChoiceButtonView> buttons;
        public CountDownView countDown;

        public ReactiveCommand onAfterEnter;
        public GameSet gameSet;
        
        public PhraseSoundPlayer phraseSoundPlayer;
    }

    private Ctx _ctx;
    private CompositeDisposable _disposables;
    private Phrase _currentPhrase;
    private ReactiveCommand<int> _onClickChoiceButton;

    private bool _choiceDone;

    public LevelScenePm(Ctx ctx)
    {
        Debug.Log($"[{this}] constructor");

        _ctx = ctx;
        _disposables = new CompositeDisposable();

        _onClickChoiceButton = new ReactiveCommand<int>().AddTo(_disposables);
        _onClickChoiceButton.Subscribe(OnClickChoiceButton).AddTo(_disposables);

        _ctx.onClickMenuButton.Subscribe(_ => { _ctx.onSwitchScene.Execute(GameScenes.Menu); }).AddTo(_disposables);
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
        
        await ShowIntro();
        
        RunDialogue();
    }

    private async Task ShowIntro()
    {
        // _ctx.showIntro.Execute();
        //  Task.Delay(introDelay);await
    }

    private void RunDialogue()
    {
        _currentPhrase = _ctx.dialogues.phrases.FirstOrDefault(p => p.phraseId == _ctx.profile.LastPhrase);
        if (_currentPhrase == null)
        {
            Debug.LogError($"[{this}] _ctx.profile.LastPhrase: {_ctx.profile.LastPhrase}. Not found in phrases.");
            return;
        }
        
        Observable.FromCoroutine(PhraseRoutine).Subscribe( _ =>
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

        for (int i = 0; i < _currentPhrase.choices.Count; i++)
        {
            var isBlocked = IsBlocked(_currentPhrase.choices[i]);
            _ctx.buttons[i].Show(_currentPhrase.choices[i].text, isBlocked);
        }
        
        Observable.FromCoroutine(ChoiceRoutine).Subscribe( _ =>
        {
            Debug.Log($"[{this}] Choice coroutine end");
        }).AddTo(_disposables);
    }

    private bool IsBlocked(Choice choice)
    {
        if (choice.ifSelected)
        {
            return !_ctx.profile.ContainsChoice(choice.requiredChoices);
        }

        return false;
    }
    
    private IEnumerator ChoiceRoutine()
    {
        var timer = 0f;

        var time = _currentPhrase.overrideChoicesDuration
            ? _currentPhrase.choicesDuration
            : _ctx.gameSet.choicesDuration;
        
        _ctx.countDown.Show(time);
        
        yield return new WaitForSeconds(_ctx.gameSet.buttonsAppearDuration);
        while (timer <= time)
        {
            yield return null;
            timer += Time.deltaTime;
            
            if(_choiceDone)
                yield break;
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
        
        OnClickChoiceButton(index);
    }

    private async Task NextPhrase()
    {
        if (string.IsNullOrWhiteSpace(_currentPhrase.nextId))
        {
            Debug.LogWarning($"[{this}] nextId not set up for phrase {_currentPhrase.phraseId}");
            return;
        }
        
        _ctx.profile.LastPhrase = _currentPhrase.nextId;
        await _ctx.phraseSoundPlayer.TryLoadDialogue(_ctx.profile.LastPhrase);
        RunDialogue();
    }

    private IEnumerator PhraseRoutine()
    {
        if (_currentPhrase == null)
        {
            Debug.LogError($"[{this}] No phrase found for id {_ctx.profile.LastPhrase}");
            yield break;
        }

        var timer = 0f;
        
        var pEvents = new List<DialogueEvent>();
        if (_currentPhrase.addEvent)
            pEvents.AddRange(_currentPhrase.dialogueEvents);

        Debug.Log($"[{this}] Execute event for phrase {_currentPhrase.phraseId}");
        _ctx.onShowPhrase.Execute(_currentPhrase);
        _ctx.phraseSoundPlayer.TryPlayAudioFile();

        while (timer <= _currentPhrase.Duration)
        {
            yield return null;

            for (var i = pEvents.Count - 1; i >= 0; i--)
            {
                var pEvent = pEvents[i];
                if (timer >= pEvent.delay)
                    _ctx.onPhraseEvent.Execute(pEvent.eventId); // todo should be event class executed
            }

            timer += Time.deltaTime;
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
        
        _ctx.countDown.Stop(_ctx.gameSet.fastButtonFadeDuration);
        _choiceDone = true;
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
}