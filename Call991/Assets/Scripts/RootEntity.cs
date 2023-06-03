using System;
using Configs;
using Core;
using I2.Loc;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RootEntity : IDisposable
{
    public struct Ctx
    {
        public AudioManager AudioManager;
        public VideoManager VideoManager;
        public OverridenDialogue OverridenDialogue;
        public Image VideoFade;
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

        var musicPath = "Sounds/Music";

        var gameSet = Resources.Load<GameSet>("GameSet");
        var audioMixer = Resources.Load<AudioMixer>("AudioMixer");
        var cursorSettings = Resources.Load<CursorSet>("CursorSet");
        var clickImage = Resources.Load<GameObject>("ClickPointImage");

        _onStartApplicationSwitchScene = new ReactiveCommand().AddTo(_disposables);
        
        var onSwitchScene = new ReactiveCommand<GameScenes>().AddTo(_disposables);
        var onScreenFade = new ReactiveCommand<(bool show, float time)>();
        var onShowTitle = new ReactiveCommand<(bool show, string[] keys)>();
        var onShowWarning = new ReactiveCommand<(bool show, string[] keys, float delayTime, float fadeTime)>();
        
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
        var blocker = new Blocker(_ctx.ScreenFade, _ctx.VideoFade, gameSet, onScreenFade.AddTo(_disposables));
        
        _ctx.AudioManager.SetCtx(new AudioManager.Ctx
        {
            GameSet = gameSet,
            playerProfile = profile,
            audioMixer = audioMixer,
            musicPath = musicPath,
        });
        _ctx.AudioManager.PlayMusic("Intro").Forget();

        _ctx.VideoManager.SetCtx(new VideoManager.Ctx
        {
        });
        
        var startApplicationSceneName = SceneManager.GetActiveScene().name;

        var scenesHandler = new ScenesHandler(new ScenesHandler.Ctx
        {
            GameSet = gameSet,
            StartApplicationSceneName = startApplicationSceneName,
            OnStartApplicationSwitchScene = _onStartApplicationSwitchScene,
            onSwitchScene = onSwitchScene,
            Profile = profile,
            AudioManager = _ctx.AudioManager,
            VideoManager = _ctx.VideoManager,
            Blocker = blocker,
            ObjectEvents = objectEvents,
            CursorSettings = cursorSettings,
            OverridenDialogue = _ctx.OverridenDialogue,
        }).AddTo(_disposables);

        var sceneSwitcher = new SceneSwitcher(new SceneSwitcher.Ctx
        {
            ScenesHandler = scenesHandler,
            OnSwitchScene = onSwitchScene,
            VideoManager = _ctx.VideoManager,
            GameSet = gameSet,
            Blocker = blocker,
            CursorSettings = cursorSettings,
            OverridenDialogue = _ctx.OverridenDialogue,
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