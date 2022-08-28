using System;
using UniRx;
using UnityEngine;

public class LevelScenePm : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<GameScenes> onSwitchScene;
        public ReactiveCommand onClickMenuButton;
    }

    private Ctx _ctx;
    private CompositeDisposable _disposables;

    public LevelScenePm(Ctx ctx)
    {
        _ctx = ctx;
        _disposables = new CompositeDisposable();
        
        _ctx.onClickMenuButton.Subscribe(_ =>
        {
            _ctx.onSwitchScene.Execute(GameScenes.Menu);
        }).AddTo(_disposables);
        
        CreateObjects();
        
        Debug.Log($"[{this}] constructor finished");
    }

    private void CreateObjects()
    {
       
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}