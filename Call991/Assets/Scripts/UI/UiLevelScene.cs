using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiLevelScene : MonoBehaviour, IDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand onClickMenuButton;
            public ReactiveCommand<string> onPhraseEvent;
            public ReactiveCommand<Phrase> onShowPhrase;
            public ReactiveCommand<Phrase> onHidePhrase;

            public Pool pool;
        }

        private const float FADE_TIME = 0.3f;

        private Ctx _ctx;

        [SerializeField] private Button menuButton = default;
        [SerializeField] private List<PersonView> persons = default;
        [SerializeField] private List<ChoiceButtonView> buttons = default;
        [SerializeField] private CountDownView countDown = default;

        private CompositeDisposable _disposables;
        public List<ChoiceButtonView> Buttons => buttons;
        public CountDownView CountDown => countDown;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            menuButton.onClick.AddListener(() => { _ctx.onClickMenuButton.Execute(); });

            _ctx.onPhraseEvent.Subscribe(OnPhraseEvent).AddTo(_disposables);
            _ctx.onShowPhrase.Subscribe(OnShowPhrase).AddTo(_disposables);
            _ctx.onHidePhrase.Subscribe(OnHidePhrase).AddTo(_disposables);

            foreach (var person in persons)
                person.gameObject.SetActive(false);

            foreach (var button in Buttons)
                button.gameObject.SetActive(false);
            
            countDown.gameObject.SetActive(false);
        }

        private void OnPhraseEvent(string eventId)
        {
            // todo for extra events on phrase time points 
        }

        private void OnShowPhrase(Phrase phrase)
        {
            var personView = persons.FirstOrDefault(p => p.ScreenPlace == phrase.screenPlace);
            if (personView == null)
            {
                Debug.LogError($"[{this}] [OnShowPhrase] no person on side {phrase.screenPlace}");
                return;
            }

            personView.ShowPhrase(phrase);
        }

        private void OnHidePhrase(Phrase phrase)
        {
            var personView = persons.FirstOrDefault(p => p.ScreenPlace == phrase.screenPlace);
            if (personView == null)
            {
                Debug.LogError($"[{this}] [OnHidePhrase] no person on side {phrase.screenPlace}");
                return;
            }

            if (phrase.hidePhraseOnEnd)
                personView.HidePhrase();

            if (phrase.hidePersonOnEnd)
                personView.gameObject.SetActive(false);
        }

        public void Dispose()
        {
        }
    }
}