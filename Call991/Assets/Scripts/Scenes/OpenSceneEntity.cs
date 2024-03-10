using UI;
using UniRx;
using UnityEngine;

public class OpenSceneEntity : IGameScene
{
    public struct Ctx
    {
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public Blocker Blocker;
        public CursorSet CursorSettings;
        public WwiseAudio AudioManager;
        public GameLevelsService GameLevelsService;
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
        _ui = Object.FindObjectOfType<UiOpening>();

        var onClickStartGame = new ReactiveCommand().AddTo(_disposables);
        onClickStartGame.Subscribe(_=>
        {
            _ctx.OnSwitchScene.Execute(GameScenes.Menu);
        }).AddTo(_disposables);
        
        _ui.SetCtx(new UiOpening.Ctx
        {
            OnClickStartGame = onClickStartGame,
            Blocker = _ctx.Blocker,
            CursorSettings = _ctx.CursorSettings,
            AudioManager = _ctx.AudioManager,
            GameLevelsService = _ctx.GameLevelsService,
        });
    }

    public void Exit()
    {
    }
}