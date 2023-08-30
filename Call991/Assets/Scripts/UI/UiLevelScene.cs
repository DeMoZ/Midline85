using System;
using System.Threading;
using System.Threading.Tasks;
using AaDialogueGraph;
using Configs;
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
    
    public class UiImagePhraseData
    {
        public string Description;

        public Sprite Sprite;
        public Phrase Phrase;
        public ImagePersonVisualData PersonVisualData;
        public PhraseVisualData PhraseVisualData;
    }

    public class UiLevelScene : MonoBehaviour, IDisposable
    {
        public struct Ctx
        {
            public GameSet GameSet;
            public DialogueService DialogueService;

            public ReactiveCommand<StatisticsData> OnLevelEnd;
            public ReactiveCommand OnClickMenuButton;
            public ReactiveCommand OnClickNextLevelButton;

            public ReactiveCommand<(bool show, string[] keys)> OnShowTitle;
            public ReactiveCommand<(bool show, string[] keys, float delayTime, float fadeTime)> OnShowWarning;

            public ReactiveCommand<bool> OnClickPauseButton;
            public ReactiveProperty<bool> IsPauseAllowed;

            public PlayerProfile Profile;
            public LevelSceneObjectsService LevelSceneObjectsService;
        }

        private Ctx _ctx;

        [Space] [SerializeField] private LevelTitleView levelTitleView = default;
        [SerializeField] private LevelView levelView = default;
        [SerializeField] private StatisticsView statisticView = default;
        [SerializeField] private PhotoView newspaper = default;
        [SerializeField] private LevelPauseView levelPauseView = default;
        [SerializeField] private UiMenuSettings menuSettings = default;
        [SerializeField] private UiLevelWarning levelWarning = default;

        private CompositeDisposable _disposables;
        private bool _isNewspaperActive;
        private ReactiveCommand _onClickToMenu;
        
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
                OnClickMenuButton = _ctx.OnClickMenuButton,
                OnClickNextLevelButton = _ctx.OnClickNextLevelButton,
            });

            levelView.SetCtx(new LevelView.Ctx
            {
                GameSet = _ctx.GameSet,
                OnClickPauseButton = onClickPauseButton,
                LevelSceneObjectsService = _ctx.LevelSceneObjectsService,

            });

            levelPauseView.SetCtx(new LevelPauseView.Ctx
            {
                OnClickMenuButton = _ctx.OnClickMenuButton,
                OnClickSettingsButton = onClickSettingsButton,
                OnClickUnPauseButton = onClickUnPauseButton,
            });

            menuSettings.SetCtx(new UiMenuSettings.Ctx
            {
                OnClickToMenu = _onClickToMenu,
                Profile = _ctx.Profile,
            });

            _ctx.DialogueService.OnShowPhrase.Subscribe(levelView.OnShowPhrase).AddTo(_disposables);
            _ctx.DialogueService.OnShowImagePhrase.Subscribe(levelView.OnShowImagePhrase).AddTo(_disposables);
            _ctx.DialogueService.OnHidePhrase.Subscribe(levelView.OnHidePhrase).AddTo(_disposables);
            _ctx.DialogueService.OnHideImagePhrase.Subscribe(levelView.OnHideImagePhrase).AddTo(_disposables);
            _ctx.DialogueService.OnShowNewspaper.Subscribe(OnShowNewspaper).AddTo(_disposables);
            _ctx.DialogueService.OnShowLevelUi.Subscribe(_ => OnShowLevelUi()).AddTo(_disposables);
            
            _ctx.OnShowTitle.Subscribe(OnShowTitle).AddTo(_disposables);
            _ctx.OnLevelEnd.Subscribe(OnLevelEnd).AddTo(_disposables);
            _ctx.OnShowWarning.Subscribe(OnShowWarning).AddTo(_disposables);
            
            onClickPauseButton.Subscribe(_ => OnClickPauseButton(true));
            onClickUnPauseButton.Subscribe(_ => OnClickPauseButton(false));
            onClickSettingsButton.Subscribe(_ => EnableUi(menuSettings.GetType()));
        }

        private void OnClickToMenu() =>
            EnableUi(levelPauseView.GetType());
        
        private void OnClickPauseButton(bool value)
        {
            if(_ctx.IsPauseAllowed.Value)
            {
                _ctx.OnClickPauseButton.Execute(value);
                EnableUi(value ? levelPauseView.GetType() : levelView.GetType());
            }
        }

        private void OnShowLevelUi()
        {
            EnableUi(levelView.GetType());
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
            levelWarning.gameObject.SetActive(levelWarning.GetType() == type);
        }

        private void OnNewspaperClose()
        {
            _isNewspaperActive = false;
        }

        private void OnShowNewspaper((Container<bool> btnPressed, Sprite sprite) spriteData)
        {
            newspaper.SetNewspaper(spriteData.sprite);
            EnableUi(newspaper.GetType());
            YieldNewspaper(spriteData.btnPressed);
        }

        private async void YieldNewspaper(Container<bool> btnPressed)
        {
            _isNewspaperActive = true;

            while (_isNewspaperActive)
                await Task.Delay(10);

            btnPressed.Value = true;
        }

        private async void OnLevelEnd(StatisticsData data)
        {
            await statisticView.SetStatistic(data);
            if (_tokenSource.IsCancellationRequested) return;

            EnableUi(statisticView.GetType());
        }

        private void OnShowTitle((bool show, string[] keys) data)
        {
            levelTitleView.Set(chapter: data.keys[0], title: data.keys[1]);
            EnableUi(data.show ? levelTitleView.GetType() : levelView.GetType());
        }
        
        private void OnShowWarning((bool show, string[] keys, float delayTime, float fadeTime) data)
        {
            levelWarning.Set(data.keys, data.delayTime, data.fadeTime);
            EnableUi(data.show ? levelWarning.GetType() : levelView.GetType());
        }

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