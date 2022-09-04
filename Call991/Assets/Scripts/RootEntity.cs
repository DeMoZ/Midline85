using System;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

public class RootEntity : IDisposable
{
    public struct Ctx
    {
    }

    private Ctx _ctx;
    private CompositeDisposable _diposables;

    public RootEntity(Ctx ctx)
    {
        Debug.Log($"[RootEntity][time] Loading scene start.. {Time.realtimeSinceStartup}");
        _ctx = ctx;
        _diposables = new CompositeDisposable();

        var savedProfile = PlayerPrefs.GetString("Profile", null);
        var profile = string.IsNullOrWhiteSpace(savedProfile)
            ? new PlayerProfile()
            : JsonConvert.DeserializeObject<PlayerProfile>(savedProfile);

        var startApplicationSceneName = SceneManager.GetActiveScene().name;
        var onStartApplicationSwitchScene = new ReactiveCommand().AddTo(_diposables);
        var onSwitchScene = new ReactiveCommand<GameScenes>().AddTo(_diposables);

        var scenesHandler = new ScenesHandler(new ScenesHandler.Ctx
        {
            startApplicationSceneName = startApplicationSceneName,
            onStartApplicationSwitchScene = onStartApplicationSwitchScene,
            onSwitchScene = onSwitchScene,
            profile = profile,
        }).AddTo(_diposables);

        var sceneSwitcher = new SceneSwitcher(new SceneSwitcher.Ctx
        {
            scenesHandler = scenesHandler,
            onSwitchScene = onSwitchScene,
        }).AddTo(_diposables);

        onStartApplicationSwitchScene.Execute();
    }

    public void Dispose()
    {
        _diposables.Dispose();
    }
}