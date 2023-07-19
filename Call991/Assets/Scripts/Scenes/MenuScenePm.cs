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
    private CompositeDisposable _disposables;

    public MenuScenePm(Ctx ctx)
    {
        _ctx = ctx;
        _disposables = new CompositeDisposable();

        _ctx.OnClickPlayGame.Subscribe(_ => OnClickPlayGame()).AddTo(_disposables);
        _ctx.OnClickNewGame.Subscribe(_ => OnClickNewGame()).AddTo(_disposables);
    }

    private void OnClickPlayGame()
    {
        Debug.Log("[MenuScenePm] OnClickPlay");
        _ctx.OnSwitchScene.Execute(GameScenes.Level);
    }

    private void OnClickNewGame()
    {
        Debug.Log("[MenuScenePm] OnClickNewGame");
        _ctx.Profile.Clear();
        _ctx.OnSwitchScene.Execute(GameScenes.Level);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}