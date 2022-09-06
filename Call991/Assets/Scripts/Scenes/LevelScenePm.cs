using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Configs;
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
    }

    private const float DeltaTime = 0.01f;
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

    private void RunDialogue()
    {
        if (string.IsNullOrWhiteSpace(_ctx.profile.Data.lastPhraseId))
            _ctx.profile.SetLastPhrase(_ctx.dialogues.phrases[0].phraseId);

        _currentPhrase = _ctx.dialogues.phrases.FirstOrDefault(p => p.phraseId == _ctx.profile.Data.lastPhraseId);

        Observable.FromCoroutine(PhraseRoutine).Subscribe( _ =>
        {
            Debug.Log($"[{this}] Coroutine end");

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

    private async void NextChoices()
    {
        if (_currentPhrase.choices.Count == 0)
        {
            Debug.LogWarning($"[{this}] no choices set up for phrase {_currentPhrase.phraseId}");
            return;
        }

        _choiceDone = false;

        for (int i = 0; i < _currentPhrase.choices.Count; i++)
            _ctx.buttons[i].Show(_currentPhrase.choices[i].description);

        await Task.Delay((int) (_ctx.gameSet.buttonsAppearDuration * 1000));
        
        Observable.FromCoroutine(ChoiceRoutine).Subscribe( _ =>
        {
            Debug.Log($"[{this}] Choice coroutine end");
        }).AddTo(_disposables);
    }
    
    private IEnumerator ChoiceRoutine()
    {
        var time = _currentPhrase.overrideChoicesDuration
            ? _currentPhrase.choicesDuration
            : _ctx.gameSet.choicesDuration;
        var timer = 0f;
        
        _ctx.countDown.Show(time);

        while (timer <= _currentPhrase.duration)
        {
            yield return new WaitForSeconds(DeltaTime);
            timer += DeltaTime;
            
            if(_choiceDone)
                yield break;
        }
        
        Debug.LogWarning($"[{this}] ch0ice time up!");
        OnClickChoiceButton(Random.Range(0, _currentPhrase.choices.Count));
    }
    
     private async void ShowCountDown() // todo make coroutine ???
     {
         var time = _currentPhrase.overrideChoicesDuration
             ? _currentPhrase.choicesDuration
             : _ctx.gameSet.choicesDuration;
         
         var countTime = 0f;
         _ctx.countDown.Show(time);

         while (!_choiceDone)
         {
             await Task.Delay((int) (DeltaTime * 1000));
             countTime += DeltaTime;
             if (countTime >= time)
             {
                 Debug.LogWarning($"{countTime}; {time}");
                 OnClickChoiceButton(Random.Range(0, _currentPhrase.choices.Count));
             }
         }
     }
    
    private void NextPhrase()
    {
        if (string.IsNullOrWhiteSpace(_currentPhrase.nextId))
        {
            Debug.LogWarning($"[{this}] nextId not set up for phrase {_currentPhrase.phraseId}");
            return;
        }

        if (_currentPhrase.hidePhraseOnEnd || _currentPhrase.hidePersonOnEnd)
            _ctx.onHidePhrase.Execute(_currentPhrase); // TODO can be awaitable hide

        _ctx.profile.SetLastPhrase(_currentPhrase.nextId);
        RunDialogue();
    }

    private IEnumerator PhraseRoutine()
    {
        if (_currentPhrase == null)
        {
            Debug.LogError($"[{this}] No phrase found for id {_ctx.profile.Data.lastPhraseId}");
            yield break;
        }

        var timer = 0f;
        
        var pEvents = new List<DialogueEvent>();
        if (_currentPhrase.addEvent)
            pEvents.AddRange(_currentPhrase.dialogueEvents);

        Debug.Log($"[{this}] Execute event for phrase {_currentPhrase.phraseId}");
        _ctx.onShowPhrase.Execute(_currentPhrase);

        while (timer <= _currentPhrase.duration)
        {
            yield return new WaitForSeconds(DeltaTime);

            for (var i = pEvents.Count - 1; i >= 0; i--)
            {
                var pEvent = pEvents[i];
                if (timer >= pEvent.delay)
                    _ctx.onPhraseEvent.Execute(pEvent.eventId); // todo should be event class executed
            }

            timer += DeltaTime;
        }
    }

    private void OnAfterEnter()
    {
        InitButtons();
        RunDialogue();
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
        _ctx.profile.SetLastPhrase(_currentPhrase.choices[index].nextPhraseId);

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