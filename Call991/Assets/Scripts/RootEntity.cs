using System;
using System.Collections.Generic;
using Configs;
using ContentDelivery;
using Core;
using I2.Loc;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class RootEntity : IDisposable
{
    public struct Ctx
    {
        public WwiseAudio AudioManagerPrefab;
        public ImageManager ImageManagerPrefab;
        public RectTransform ImageManagerParent;
        public VideoManager VideoManagerPrefab;
        public RectTransform VideoManagerParent;
        public FilmProjector FilmProjectorPrefab;
        public Transform FilmProjectorParent;
        
        public OverridenDialogue OverridenDialogue;
        public Image ScreenFade;
        public Transform ClicksParent;
    }

    private Ctx _ctx;
    private CompositeDisposable _disposables;
    private readonly ReactiveCommand _onStartApplicationSwitchScene;

    public RootEntity(Ctx ctx)
    {
        Debug.Log($"[RootEntity][time] Loading scene start.. {Time.realtimeSinceStartup}");
        _ctx = ctx;
        _disposables = new CompositeDisposable();

        var gameSet = Resources.Load<GameSet>("GameSet");
        var cursorSettings = Resources.Load<CursorSet>("CursorSet");
        var clickImage = Resources.Load<GameObject>("ClickPointImage");

        _onStartApplicationSwitchScene = new ReactiveCommand().AddTo(_disposables);
        
        var levelLanguages = new ReactiveProperty<List<string>>().AddTo(_disposables);
        var isPauseAllowed = new ReactiveProperty<bool>(true).AddTo(_disposables);
        var onSwitchScene = new ReactiveCommand<GameScenes>().AddTo(_disposables);
        var onScreenFade = new ReactiveCommand<(bool show, float time)>().AddTo(_disposables);
        var onShowTitle = new ReactiveCommand<(bool show, string[] keys)>().AddTo(_disposables);
        var onShowWarning = new ReactiveCommand<(bool show, string[] keys, float delayTime, float fadeTime)>().AddTo(_disposables);
        
        var objectEvents = new ObjectEvents(new ObjectEvents.Ctx
        {
            OnScreenFade = onScreenFade,
            OnShowTitle = onShowTitle,
            OnShowWarning = onShowWarning,
            SkipTitle = _ctx.OverridenDialogue.SkipTitle,
            SkipWarning = _ctx.OverridenDialogue.SkipWarning,
        }).AddTo(_disposables);

        var profile = new PlayerProfile();
        SetLanguage(profile.TextLanguage);

        var clickPointHandler = new ClickPointHandler(clickImage, _ctx.ClicksParent).AddTo(_disposables);

        var blocker = new Blocker(new Blocker.Ctx
        {
            ScreenFade = _ctx.ScreenFade,
            GameSet = gameSet,
            OnScreenFade = onScreenFade,
            IsPauseAllowed = isPauseAllowed,
        }).AddTo(_disposables);

        var audioManager = Object.Instantiate(_ctx.AudioManagerPrefab);
        audioManager.SetCtx(new WwiseAudio.Ctx
        {
            LevelLanguages = levelLanguages,
            GameSet = gameSet,
            Profile = profile,
            OnSwitchScene = onSwitchScene,
        });
        audioManager.Initialize().Forget();

        var imageManager = Object.Instantiate(_ctx.ImageManagerPrefab, _ctx.ImageManagerParent);
        imageManager.SetCtx(new ImageManager.Ctx
        {
        });

        var videoManager = Object.Instantiate(_ctx.VideoManagerPrefab, _ctx.VideoManagerParent);
        videoManager.SetCtx(new VideoManager.Ctx
        {
        });
        
        var filmProjector = Object.Instantiate(_ctx.FilmProjectorPrefab, _ctx.FilmProjectorParent);

        var mediaService = new MediaService(new MediaService.Ctx
        {
            AudioManager = audioManager,
            ImageManager = imageManager,
            VideoManager = videoManager,
            FilmProjector = filmProjector,
        });

        var startApplicationSceneName = SceneManager.GetActiveScene().name;
        var logService = new LoggerService().AddTo(_disposables);
        var dialogueLoggerPm = new DialogueLoggerPm(logService).AddTo(_disposables);
        var addressableDownloader = new AddressableDownloader().AddTo(_disposables);
        var gameLevelsService = new GameLevelsService(gameSet, _ctx.OverridenDialogue, dialogueLoggerPm,
            addressableDownloader).AddTo(_disposables);
        
        var scenesHandler = new ScenesHandler(new ScenesHandler.Ctx
        {
            GameSet = gameSet,
            StartApplicationSceneName = startApplicationSceneName,
            OnStartApplicationSwitchScene = _onStartApplicationSwitchScene,
            OnSwitchScene = onSwitchScene,
            Profile = profile,
            MediaService = mediaService,
            Blocker = blocker,
            ObjectEvents = objectEvents,
            CursorSettings = cursorSettings,
            IsPauseAllowed = isPauseAllowed,
            LevelLanguages = levelLanguages,
            GameLevelsService = gameLevelsService,
        }).AddTo(_disposables);

        var sceneSwitcher = new SceneSwitcher(new SceneSwitcher.Ctx
        {
            ScenesHandler = scenesHandler,
            OnSwitchScene = onSwitchScene,
            CursorSettings = cursorSettings,
        }).AddTo(_disposables);

        _onStartApplicationSwitchScene.Execute();
    }

    private void SetLanguage(string language)
    {
        LocalizationManager.CurrentLanguage = language;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}