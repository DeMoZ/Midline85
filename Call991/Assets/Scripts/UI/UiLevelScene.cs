using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using PhotoViewer.Scripts.Photo;
using UniRx;
using UnityEngine;

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
            public ReactiveCommand<List<StatisticElement>> onPopulateStatistics;

            public ReactiveCommand<(Container<Task> task, Sprite sprite)> onShowNewspaper;
            public ReactiveCommand<bool> onClickPauseButton;

            public Pool pool;
        }
        
        private Ctx _ctx;

        [SerializeField] private MenuButtonView pauseButton = default;
        [SerializeField] private List<PersonView> persons = default;
        [SerializeField] private List<ChoiceButtonView> buttons = default;
        [SerializeField] private CountDownView countDown = default;
        [SerializeField] private AudioSource phraseAudioSource = default;
        [Space] 
        [SerializeField] private LevelTitleView levelTitleView = default;
        [SerializeField] private LevelView levelView = default;
        [SerializeField] private StatisticsView statisticView = default;
        [SerializeField] private PhotoView newspaper = default;
        [SerializeField] private LevelPauseView levelPauseView = default;

        private CompositeDisposable _disposables;
        private bool _isNewspaperActive;

        public List<ChoiceButtonView> Buttons => buttons;
        public CountDownView CountDown => countDown;
        public AudioSource PhraseAudioSource => phraseAudioSource;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            pauseButton.OnClick += OnClickPauseButton;
            newspaper.OnClose += OnNewspaperClose;

            var onClickUnPauseButton = new ReactiveCommand().AddTo(_disposables);

            statisticView.SetCtx(new StatisticsView.Ctx
            {
                onClickMenuButton = _ctx.onClickMenuButton,
            });

            levelPauseView.SetCtx(new LevelPauseView.Ctx
            {
                onClickMenuButton = _ctx.onClickMenuButton,
                onClickUnPauseButton = onClickUnPauseButton,
            });

            _ctx.onPhraseSoundEvent.Subscribe(OnPhraseSoundEvent).AddTo(_disposables);
            _ctx.onShowPhrase.Subscribe(OnShowPhrase).AddTo(_disposables);
            _ctx.onHidePhrase.Subscribe(OnHidePhrase).AddTo(_disposables);
            _ctx.onShowIntro.Subscribe(OnShowIntro).AddTo(_disposables);
            _ctx.onHideLevelUi.Subscribe(OnHideLevelUi).AddTo(_disposables);
            _ctx.onShowStatisticUi.Subscribe(OnShowStatisticUi).AddTo(_disposables);
            _ctx.onPopulateStatistics.Subscribe(OnPopulateStatistics).AddTo(_disposables);
            _ctx.onShowNewspaper.Subscribe(OnShowNewspaper).AddTo(_disposables);

            onClickUnPauseButton.Subscribe(_ => OnClickPauseButton(false));

            foreach (var person in persons)
            {
                person.gameObject.SetActive(false);
            }

            foreach (var button in Buttons)
                button.gameObject.SetActive(false);

            countDown.gameObject.SetActive(false);
        }
        
        private void OnClickPauseButton() => OnClickPauseButton(true);

        private void OnClickPauseButton(bool value)
        {
            _ctx.onClickPauseButton.Execute(value);
            EnableUi(value ? levelPauseView.GetType() : levelView.GetType());
        }

        private void EnableUi(Type type)
        {
            if (levelView == null) return;
            
            levelTitleView.gameObject.SetActive(levelTitleView.GetType() == type);
            levelView.gameObject.SetActive(levelView.GetType() == type);
            statisticView.gameObject.SetActive(statisticView.GetType() == type);
            newspaper.gameObject.SetActive(newspaper.GetType() == type);
            levelPauseView.gameObject.SetActive(levelPauseView.GetType() == type);
        }

        private void OnNewspaperClose()
        {
            _isNewspaperActive = false;
        }

        private void OnShowNewspaper((Container<Task> task, Sprite sprite) spriteData)
        {
            newspaper.SetNewspaper(spriteData.sprite);
            EnableUi(newspaper.GetType());
            spriteData.task.Value = YieldNewspaper();
        }

        private async Task YieldNewspaper()
        {
            _isNewspaperActive = true;

            while (_isNewspaperActive)
                await Task.Delay(10);
        }

        private void OnHideLevelUi(float time)
        {
            Debug.LogWarning($"[{this}] OnHideLevelUi");

            levelView.CanvasGroup.DOFade(0, time).OnComplete(() => { EnableUi(statisticView.GetType()); });
        }

        private void OnShowStatisticUi(float time)
        {
            statisticView.Fade(time);
        }

        private void OnPopulateStatistics(List<StatisticElement> statistics)
        {
            statisticView.PopulateCells(statistics);
        }

        private void OnShowIntro(bool show) => 
            EnableUi(show ? levelTitleView.GetType() : levelView.GetType());

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

        public void Dispose()
        {
            newspaper.OnClose -= OnClickPauseButton;
            pauseButton.OnClick -= OnNewspaperClose;
        }
    }
}