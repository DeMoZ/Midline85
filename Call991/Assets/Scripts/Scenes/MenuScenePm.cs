using System;
using UniRx;
using UnityEngine;


public class MenuScenePm : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand onClickPlayGame;
        public ReactiveCommand onClickNewGame;
        public ReactiveCommand onClickSettings;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public PlayerProfile profile;
    }

    private Ctx _ctx;

    public MenuScenePm(Ctx ctx)
    {
        _ctx = ctx;

        _ctx.onClickPlayGame.Subscribe(_ => OnClickPlayGame());
        _ctx.onClickNewGame.Subscribe(_ => OnClickNewGame());
        _ctx.onClickSettings.Subscribe(_ => OnClickSettings());
    }

    private void OnClickPlayGame()
    {
        Debug.Log("[MenuScenePm] OnClickPlay");
        _ctx.onSwitchScene.Execute(GameScenes.Level1);
    }

    private void OnClickNewGame()
    {
        Debug.Log("[MenuScenePm] OnClickNewGame");
        _ctx.profile.Clear();
        _ctx.onSwitchScene.Execute(GameScenes.Level1);
    }

    private void OnClickSettings()
    {
        Debug.Log("[MenuScenePm] OnClickSettings");
    }

    public void Dispose()
    {
    }
}