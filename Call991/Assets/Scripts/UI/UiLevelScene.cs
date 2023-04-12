using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AaDialogueGraph;
using PhotoViewer.Scripts.Photo;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiPhraseData
    {
        public float DefaultTime;
        public string Description;
        
        public Phrase Phrase;
        public PersonVisualData PersonVisualData;
        public PhraseVisualData PhraseVisualData;
    }
    
    public class UiLevelScene : MonoBehaviour, IDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<UiPhraseData> OnShowPhrase;
            
            public ReactiveCommand onClickMenuButton;
            //public ReactiveCommand<PhraseEvent> onPhraseSoundEvent;
            //public ReactiveCommand<PhraseSet> onShowPhrase;
            public ReactiveCommand<UiPhraseData> onHidePhrase;
            public ReactiveCommand<bool> onShowIntro;

            public ReactiveCommand<float> onHideLevelUi;
            public ReactiveCommand<float> onShowStatisticUi;
            public ReactiveCommand<List<StatisticElement>> onPopulateStatistics;

            public ReactiveCommand<(Container<Task> task, Sprite sprite)> onShowNewspaper;
            public ReactiveCommand<bool> onClickPauseButton;

            public Pool pool;
            public AudioManager audioManager;
            public PlayerProfile profile;
        }

        private Ctx _ctx;

        [SerializeField] private AudioSource phraseAudioSource = default;
        [Space] [SerializeField] private LevelTitleView levelTitleView = default;
        [SerializeField] private LevelView levelView = default;
        [SerializeField] private StatisticsView statisticView = default;
        [SerializeField] private PhotoView newspaper = default;
        [SerializeField] private LevelPauseView levelPauseView = default;
        [SerializeField] private UiMenuSettings menuSettings = default;

        private CompositeDisposable _disposables;
        private bool _isNewspaperActive;
        private ReactiveCommand _onClickToMenu;

        public List<ChoiceButtonView> Buttons => levelView.Buttons;
        public CountDownView CountDown => levelView.CountDown;
        public AudioSource PhraseAudioSource => phraseAudioSource;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            _onClickToMenu = new ReactiveCommand();
            _onClickToMenu.Subscribe(_ => OnClickToMenu()).AddTo(_disposables);

            newspaper.OnClose += OnNewspaperClose;

            var onClickPauseButton = new ReactiveCommand().AddTo(_disposables);
            var onClickUnPauseButton = new ReactiveCommand().AddTo(_disposables);
            var onClickSettingsButton = new ReactiveCommand().AddTo(_disposables);

            statisticView.SetCtx(new StatisticsView.Ctx
            {
                onClickMenuButton = _ctx.onClickMenuButton,
            });

            levelView.SetCtx(new LevelView.Ctx
            {
                onClickPauseButton = onClickPauseButton,
            });

            levelPauseView.SetCtx(new LevelPauseView.Ctx
            {
                onClickMenuButton = _ctx.onClickMenuButton,
                onClickSettingsButton = onClickSettingsButton,
                onClickUnPauseButton = onClickUnPauseButton,
            });

            menuSettings.SetCtx(new UiMenuSettings.Ctx
            {
                audioManager = _ctx.audioManager,
                onClickToMenu = _onClickToMenu,
                profile = _ctx.profile,
            });

            _ctx.OnShowPhrase.Subscribe(levelView.OnShowPhrase).AddTo(_disposables);
            
            //_ctx.onShowPhrase.Subscribe(levelView.OnShowPhrase).AddTo(_disposables);
            _ctx.onHidePhrase.Subscribe(levelView.OnHidePhrase).AddTo(_disposables);
            _ctx.onShowIntro.Subscribe(OnShowIntro).AddTo(_disposables);
            _ctx.onHideLevelUi.Subscribe(time =>
            {
                levelView.OnHideLevelUi(time, () => { EnableUi(statisticView.GetType()); });
            }).AddTo(_disposables);
            _ctx.onShowStatisticUi.Subscribe(OnShowStatisticUi).AddTo(_disposables);
            _ctx.onPopulateStatistics.Subscribe(OnPopulateStatistics).AddTo(_disposables);
            _ctx.onShowNewspaper.Subscribe(OnShowNewspaper).AddTo(_disposables);

            onClickPauseButton.Subscribe(_ => OnClickPauseButton(true));
            onClickUnPauseButton.Subscribe(_ => OnClickPauseButton(false));
            onClickSettingsButton.Subscribe(_ => EnableUi(menuSettings.GetType()));
        }

        private void OnClickToMenu() =>
            EnableUi(levelPauseView.GetType());

        private void OnClickPauseButton(bool value)
        {
            _ctx.onClickPauseButton.Execute(value);
            EnableUi(value ? levelPauseView.GetType() : levelView.GetType());
        }

        private void EnableUi(Type type)
        {
            if (levelView == null) return;

            menuSettings.gameObject.SetActive(menuSettings.GetType() == type);
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

        public void Dispose()
        {
            newspaper.OnClose -= OnNewspaperClose;

            //levelTitleView.Dispose();
            levelView.Dispose();
            //statisticView.Dispose();
            //newspaper.Dispose();
            levelPauseView.Dispose();
            //menuSettings.Dispose();
        }
    }
}