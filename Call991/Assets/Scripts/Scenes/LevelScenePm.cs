using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

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
    }

    private Ctx _ctx;
    private CompositeDisposable _disposables;
    private Phrase _currentPhrase;

    public LevelScenePm(Ctx ctx)
    {
        Debug.Log($"[{this}] constructor");

        _ctx = ctx;
        _disposables = new CompositeDisposable();

        _ctx.onClickMenuButton.Subscribe(_ => { _ctx.onSwitchScene.Execute(GameScenes.Menu); }).AddTo(_disposables);

        StartScene();
    }

    private async void StartScene()
    {
        await Task.Yield();

        RunDialogue();
    }

    private void RunDialogue()
    {
        // float timer = 0;
        // float mSeconds = 1;

        if (string.IsNullOrWhiteSpace(_ctx.profile.Data.lastPhraseId))
            _ctx.profile.Data.lastPhraseId = _ctx.dialogues.phrases[0].phraseId;

        _currentPhrase = _ctx.dialogues.phrases.FirstOrDefault(p => p.phraseId == _ctx.profile.Data.lastPhraseId);

        Observable.FromCoroutine(PhraseRoutine).Subscribe(_ =>
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

    private void NextChoices()
    {
        if (_currentPhrase.choices.Count == 0)
        {
            Debug.LogWarning($"[{this}] no choices set up for phrase {_currentPhrase.phraseId}");
            return;
        }

        Debug.LogWarning($"[{this}] SHOW choices ... in progress");
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

        _ctx.profile.Data.lastPhraseId = _currentPhrase.nextId;
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
        var deltaTime = 0.01f;
        var pEvents = new List<DialogueEvent>();
        if (_currentPhrase.addEvent)
            pEvents.AddRange(_currentPhrase.dialogueEvents);

        Debug.Log($"[{this}] Execute event for phrase {_currentPhrase.phraseId}");
        _ctx.onShowPhrase.Execute(_currentPhrase);

        while (timer <= _currentPhrase.duration)
        {
            yield return new WaitForSeconds(deltaTime);

            for (var i = pEvents.Count - 1; i >= 0; i--)
            {
                var pEvent = pEvents[i];
                if (timer >= pEvent.delay)
                    _ctx.onPhraseEvent.Execute(pEvent.eventId); // todo should be event class executed
            }

            timer += deltaTime;
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}