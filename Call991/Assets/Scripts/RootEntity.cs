using System;
using I2.Loc;
using UniRx;
using UnityEngine;
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

        var profile = new PlayerProfile();
        SetLanguage(profile.TextLanguage);

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