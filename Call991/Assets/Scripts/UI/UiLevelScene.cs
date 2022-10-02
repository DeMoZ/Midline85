using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
            
            public ReactiveCommand<float> onHideLevelUi;
            public ReactiveCommand<float> onShowStatisticUi;
            
            public Pool pool;
            public ReactiveCommand<List<StatisticElement>> onPopulateStatistics;
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
        [SerializeField] private CanvasGroup levelUiGroup = default;
        [SerializeField] private StatisticsView statisticView = default;

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
            statisticView.SetCtx(new StatisticsView.Ctx
            {
                onClickMenuButton = _ctx.onClickMenuButton,
            });
            
            _ctx.onPhraseSoundEvent.Subscribe(OnPhraseSoundEvent).AddTo(_disposables);
            _ctx.onShowPhrase.Subscribe(OnShowPhrase).AddTo(_disposables);
            _ctx.onHidePhrase.Subscribe(OnHidePhrase).AddTo(_disposables);
            _ctx.onShowIntro.Subscribe(OnShowIntro).AddTo(_disposables);
            _ctx.onHideLevelUi.Subscribe(OnHideLevelUi).AddTo(_disposables);
            _ctx.onShowStatisticUi.Subscribe(OnShowStatisticUi).AddTo(_disposables);
            _ctx.onPopulateStatistics.Subscribe(OnPopulateStatistics).AddTo(_disposables);
            
            foreach (var person in persons)
                person.gameObject.SetActive(false);

            foreach (var button in Buttons)
                button.gameObject.SetActive(false);
            
            countDown.gameObject.SetActive(false);
        }

        private void OnHideLevelUi(float time)
        {
            Debug.LogWarning($"[{this}] OnHideLevelUi");

            levelUiGroup.DOFade(0, time).OnComplete(() =>
            {
                levelUiGroup.gameObject.SetActive(false);
                statisticView.gameObject.SetActive(true);
            });
        }
        
        private void OnShowStatisticUi(float time)
        {
            statisticView.Fade(time);
        }
        
        private void OnPopulateStatistics(List<StatisticElement> statistics)
        {
            statisticView.PopulateCells(statistics);
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