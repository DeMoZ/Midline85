using Configs;
using UI;
using UniRx;
using UnityEngine;

public class OpenSceneEntity : IGameScene
{
    public struct Ctx
    {
        public GameSet gameSet;
        public ReactiveCommand<GameScenes> onSwitchScene;
    }

    private readonly Ctx _ctx;
    private CompositeDisposable _disposables;
    private UiOpening _ui;

    public OpenSceneEntity(Ctx ctx)
    {
        _disposables = new CompositeDisposable();
        _ctx = ctx;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    public void Enter()
    {
        Debug.Log($"[{this}] Entered");
        _ui = UnityEngine.GameObject.FindObjectOfType<UiOpening>();

        var onClickStartGame = new ReactiveCommand().AddTo(_disposables);
        onClickStartGame.Subscribe(_=>
        {
            _ctx.onSwitchScene.Execute(GameScenes.Menu);
        }).AddTo(_disposables);
        
        _ui.SetCtx(new UiOpening.Ctx
        {
            gameSet = _ctx.gameSet,
            onClickStartGame = onClickStartGame,
        });
    }

    public void Exit()
    {
    }
}