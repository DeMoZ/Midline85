using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace UI
{
    public class UiLevelScene : MonoBehaviour, IDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand onClickMenuButton;
            public ReactiveCommand<PhraseEvent> onPhraseSoundEvent;
            public ReactiveCommand<PhraseSet> onShowPhrase;
            public ReactiveCommand<PhraseSet> onHidePhrase;
            public ReactiveCommand<bool> onShowIntro;
            public ReactiveCommand<List<StatisticElement>> onLevelEnd;

            public Pool pool;
        }

        private const float FADE_TIME = 0.3f;

        private Ctx _ctx;

        [SerializeField] private MenuButtonView menuButton = default;
        [SerializeField] private List<PersonView> persons = default;
        [SerializeField] private List<ChoiceButtonView> buttons = default;
        [SerializeField] private CountDownView countDown = default;
        [SerializeField] private VideoPlayer videoPlayer = default;
        [SerializeField] private AudioSource phraseAudioSource = default;
        [SerializeField] private GameObject showIntro = default;
        
        private CompositeDisposable _disposables;
        public List<ChoiceButtonView> Buttons => buttons;
        public CountDownView CountDown => countDown;
        public VideoPlayer VideoPlayer => videoPlayer;

        public AudioSource PhraseAudioSource => phraseAudioSource;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            menuButton.OnClick += OnClickMenu;

            _ctx.onPhraseSoundEvent.Subscribe(OnPhraseSoundEvent).AddTo(_disposables);
            _ctx.onShowPhrase.Subscribe(OnShowPhrase).AddTo(_disposables);
            _ctx.onHidePhrase.Subscribe(OnHidePhrase).AddTo(_disposables);
            _ctx.onShowIntro.Subscribe(OnShowIntro).AddTo(_disposables);

            foreach (var person in persons)
                person.gameObject.SetActive(false);

            foreach (var button in Buttons)
                button.gameObject.SetActive(false);
            
            countDown.gameObject.SetActive(false);
        }

        private void OnShowIntro(bool show)
        {
            showIntro.SetActive(show);
        }
        
        private void OnPhraseSoundEvent(PhraseEvent phraseEvent)
        {
            // todo: for extra sound events on phrase time points 
        }

        private void OnShowPhrase(PhraseSet phrase)
        {
            var personView = persons.FirstOrDefault(p => p.ScreenPlace == phrase.screenPlace);
            if (personView == null)
            {
                Debug.LogError($"[{this}] [OnShowPhrase] no person on side {phrase.screenPlace}");
                return;
            }

            personView.ShowPhrase(phrase);
        }

        private void OnHidePhrase(PhraseSet phrase)
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

        private void OnClickMenu() => 
            _ctx.onClickMenuButton.Execute();

        public void Dispose()
        {
            menuButton.OnClick -= OnClickMenu;
        }
    }
}