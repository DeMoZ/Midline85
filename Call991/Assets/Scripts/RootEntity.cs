using System;
using Configs;
using I2.Loc;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class RootEntity : IDisposable
{
    public struct Ctx
    {
        public AudioManager audioManager;
    }

    private Ctx _ctx;
    private CompositeDisposable _diposables;
    
    public RootEntity(Ctx ctx)
    {
        Debug.Log($"[RootEntity][time] Loading scene start.. {Time.realtimeSinceStartup}");
        _ctx = ctx;
        _diposables = new CompositeDisposable();

        var gameSet = Resources.Load<GameSet>("GameSet");

        var onAudioLanguage = new ReactiveCommand<Language>().AddTo(_diposables);
        var soundPath = "Sounds/Ui";
        var musicPath = "Sounds/Music";
        var voiceFolder = "RU"; // todo: reactive
        var levelFolder = "RU_7_P"; // todo: reactive

        _ctx.audioManager.SetCtx(new AudioManager.Ctx
        {
            gameSet = gameSet,
            onAudioLanguage = onAudioLanguage,
            audioMixer = Resources.Load<AudioMixer>("AudioMixer"),
            soundPath = soundPath,
            musicPath = musicPath,
            voiceFolder = voiceFolder,
            levelFolder = levelFolder,
        });

        _ctx.audioManager.PlayMusic("Intro");
        var profile = new PlayerProfile(onAudioLanguage);
        SetLanguage(profile.TextLanguage);

        var startApplicationSceneName = SceneManager.GetActiveScene().name;

        var onStartApplicationSwitchScene = new ReactiveCommand().AddTo(_diposables);
        var onSwitchScene = new ReactiveCommand<GameScenes>().AddTo(_diposables);

        var scenesHandler = new ScenesHandler(new ScenesHandler.Ctx
        {
            gameSet = gameSet,
            startApplicationSceneName = startApplicationSceneName,
            onStartApplicationSwitchScene = onStartApplicationSwitchScene,
            onSwitchScene = onSwitchScene,
            profile = profile,
            audioManager = _ctx.audioManager,
        }).AddTo(_diposables);

        var sceneSwitcher = new SceneSwitcher(new SceneSwitcher.Ctx
        {
            scenesHandler = scenesHandler,
            onSwitchScene = onSwitchScene,
        }).AddTo(_diposables);

        onStartApplicationSwitchScene.Execute();
    }

    private void SetLanguage(Language language)
    {
        string locLanguage = language switch
        {
            Language.EN => "English",
            Language.RU => "Russian",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };

        LocalizationManager.CurrentLanguage = locLanguage;
    }

    public void Dispose()
    {
        _diposables.Dispose();
    }
}