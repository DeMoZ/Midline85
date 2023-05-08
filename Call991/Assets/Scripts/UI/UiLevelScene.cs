using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AaDialogueGraph;
using PhotoViewer.Scripts.Photo;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiPhraseData
    {
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
            public ReactiveCommand<List<RecordData>> OnLevelEnd;
            
            public ReactiveCommand onClickMenuButton;
            public ReactiveCommand<UiPhraseData> onHidePhrase;
            public ReactiveCommand<bool> onShowIntro;
            public ReactiveCommand<(Container<Task> task, Sprite sprite)> onShowNewspaper;
            public ReactiveCommand<bool> onClickPauseButton;

            public AudioManager AudioManager;
            public PlayerProfile Profile;
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
        private CancellationTokenSource _tokenSource;
        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();
            _tokenSource = new CancellationTokenSource().AddTo(_disposables);
            
            _onClickToMenu = new ReactiveCommand();
            _onClickToMenu.Subscribe(_ => OnClickToMenu()).AddTo(_disposables);

            newspaper.OnClose += OnNewspaperClose;

            var onClickPauseButton = new ReactiveCommand().AddTo(_disposables);
            var onClickUnPauseButton = new ReactiveCommand().AddTo(_disposables);
            var onClickSettingsButton = new ReactiveCommand().AddTo(_disposables);

            statisticView.SetCtx(new StatisticsView.Ctx
            {
                OnClickMenuButton = _ctx.onClickMenuButton,
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
                audioManager = _ctx.AudioManager,
                onClickToMenu = _onClickToMenu,
                profile = _ctx.Profile,
            });

            _ctx.OnShowPhrase.Subscribe(levelView.OnShowPhrase).AddTo(_disposables);
            
            _ctx.onHidePhrase.Subscribe(levelView.OnHidePhrase).AddTo(_disposables);
            _ctx.onShowIntro.Subscribe(OnShowIntro).AddTo(_disposables);
            _ctx.OnLevelEnd.Subscribe(OnLevelEnd).AddTo(_disposables);
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

        private async void OnLevelEnd(List<RecordData> data)
        {
            await statisticView.PopulateCells(data);
            if (_tokenSource.IsCancellationRequested) return;
            
            EnableUi(statisticView.GetType()); 
        }

        private void OnShowIntro(bool show) =>
            EnableUi(show ? levelTitleView.GetType() : levelView.GetType());
        
        public void Dispose()
        {
            newspaper.OnClose -= OnNewspaperClose;
            _tokenSource.Cancel();
            //levelTitleView.Dispose();
            levelView.Dispose();
            //statisticView.Dispose();
            //newspaper.Dispose();
            levelPauseView.Dispose();
            //menuSettings.Dispose();
            _disposables.Dispose();
        }
    }
}