using System;
using UniRx;
using UnityEngine;

public class MenuScenePm : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand OnClickPlayGame;
        public ReactiveCommand OnClickNewGame;
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public PlayerProfile Profile;
    }

    private Ctx _ctx;

    public MenuScenePm(Ctx ctx)
    {
        _ctx = ctx;

        _ctx.OnClickPlayGame.Subscribe(_ => OnClickPlayGame());
        _ctx.OnClickNewGame.Subscribe(_ => OnClickNewGame());
    }

    private void OnClickPlayGame()
    {
        Debug.Log("[MenuScenePm] OnClickPlay");
        _ctx.OnSwitchScene.Execute(GameScenes.Level1);
    }

    private void OnClickNewGame()
    {
        Debug.Log("[MenuScenePm] OnClickNewGame");
        _ctx.Profile.Clear();
        _ctx.OnSwitchScene.Execute(GameScenes.Level1);
    }

    private void OnClickSettings()
    {
        Debug.Log("[MenuScenePm] OnClickSettings");
    }

    public void Dispose()
    {
    }
}