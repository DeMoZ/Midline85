using System;
using UniRx;
using UnityEngine;

public class MenuScenePm : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<int> OnLevelPlay;
        public ReactiveCommand<int> OnLevelSelect;
        
        public ReactiveCommand OnClickNewGame;
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public PlayerProfile Profile;
        public GameLevelsService GameLevelsService;
    }

    private Ctx _ctx;
    private CompositeDisposable _disposables;

    public MenuScenePm(Ctx ctx)
    {
        _ctx = ctx;
        _disposables = new CompositeDisposable();

        _ctx.OnLevelPlay.Subscribe(OnClickPlayGame).AddTo(_disposables);
        // _ctx.OnLevelSelect.Subscribe(_ => OnClickPlayGame()).AddTo(_disposables); // TODO show some level info 
        
        _ctx.OnClickNewGame.Subscribe(_ => OnClickNewGame()).AddTo(_disposables);
    }

    private void OnClickPlayGame(int index)
    {
        Debug.Log("[MenuScenePm] OnClickPlay");
        _ctx.GameLevelsService.SetLevel(index);
        _ctx.OnSwitchScene.Execute(GameScenes.Level);
    }

    private void OnClickNewGame()
    {
        Debug.Log("[MenuScenePm] OnClickNewGame");
        _ctx.Profile.Clear();
        _ctx.GameLevelsService.SetLevel(0);
       _ctx.OnSwitchScene.Execute(GameScenes.Level);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}