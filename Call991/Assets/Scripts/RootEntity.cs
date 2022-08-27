using System;
using UniRx;
using UnityEngine;

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
        
        var onSwitchScene = new ReactiveCommand<GameScenes>();

        var sceneSwitcher = new SceneSwitcher(new SceneSwitcher.Ctx
        {
            onSwitchScene = onSwitchScene,
        });

        onSwitchScene.Execute(GameScenes.Menu);
    }

    public void Dispose()
    {
        _diposables.Dispose();
    }
}