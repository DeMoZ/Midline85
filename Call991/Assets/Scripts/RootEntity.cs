using System;
using Configs;
using Core;
using I2.Loc;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class RootEntity : IDisposable
{
    public struct Ctx
    {
        public AudioManager AudioManager;
        public VideoManager VideoManager;
        public Blocker Blocker;
        public ObjectEvents ObjectEvents;
        public OverridenDialogue OverridenDialogue;
    }

    private Ctx _ctx;
    private CompositeDisposable _diposables;
    private readonly ReactiveCommand _onStartApplicationSwitchScene;

    public RootEntity(Ctx ctx)
    {
        Debug.Log($"[RootEntity][time] Loading scene start.. {Time.realtimeSinceStartup}");
        _ctx = ctx;
        _diposables = new CompositeDisposable();

        var gameSet = Resources.Load<GameSet>("GameSet");
        var audioMixer = Resources.Load<AudioMixer>("AudioMixer");
        var profile = new PlayerProfile();
        SetLanguage(profile.TextLanguage);
        
        var musicPath = "Sounds/Music";

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

        _onStartApplicationSwitchScene = new ReactiveCommand().AddTo(_diposables);
        var onSwitchScene = new ReactiveCommand<GameScenes>().AddTo(_diposables);
        var cursorSettings = Resources.Load<CursorSet>("CursorSet");
        
        var scenesHandler = new ScenesHandler(new ScenesHandler.Ctx
        {
            GameSet = gameSet,
            StartApplicationSceneName = startApplicationSceneName,
            OnStartApplicationSwitchScene = _onStartApplicationSwitchScene,
            onSwitchScene = onSwitchScene,
            Profile = profile,
            AudioManager = _ctx.AudioManager,
            videoManager = _ctx.VideoManager,
            Blocker = _ctx.Blocker,
            ObjectEvents = _ctx.ObjectEvents,
            CursorSettings = cursorSettings,
            OverridenDialogue = _ctx.OverridenDialogue,
        }).AddTo(_diposables);

        var sceneSwitcher = new SceneSwitcher(new SceneSwitcher.Ctx
        {
            ScenesHandler = scenesHandler,
            OnSwitchScene = onSwitchScene,
            VideoManager = _ctx.VideoManager,
            GameSet = gameSet,
            Blocker = _ctx.Blocker,
            CursorSettings = cursorSettings,
            OverridenDialogue = _ctx.OverridenDialogue,
        }).AddTo(_diposables);

        _onStartApplicationSwitchScene.Execute();
    }

    private void SetLanguage(string language)
    {
        LocalizationManager.CurrentLanguage = language;
    }

    public void Dispose()
    {
        _diposables.Dispose();
    }
}