using UI;
using UniRx;

public class MenuSceneEntity : IGameScene
{
    public struct Ctx
    {
        public GameScenes scene;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public PlayerProfile profile;
    }

    private Ctx _ctx;
    private UiMenuScene _ui;

    private ReactiveCommand _onClickPlayGame;
    private ReactiveCommand _onClickNewGame;
    private ReactiveCommand _onClickSettings;

    public MenuSceneEntity(Ctx ctx)
    {
        _ctx = ctx;

        _onClickPlayGame = new ReactiveCommand();
        _onClickNewGame = new ReactiveCommand();
        _onClickSettings = new ReactiveCommand();
    }

    public void Enter()
    {
        var menuScenePm = new MenuScenePm(new MenuScenePm.Ctx
        {
            onClickPlayGame = _onClickPlayGame,
            onClickNewGame = _onClickNewGame,
            onClickSettings = _onClickSettings,
            onSwitchScene = _ctx.onSwitchScene,
            profile = _ctx.profile,
        });
        
        // Find UI or instantiate from Addressable
        // _ui = Addressable.Instantiate();
        _ui = UnityEngine.GameObject.FindObjectOfType<UiMenuScene>();
        
        _ui.SetCtx(new UiMenuScene.Ctx
        {
            onClickPlayGame = _onClickPlayGame,
            onClickNewGame = _onClickNewGame,
            onClickSettings = _onClickSettings,
        });
    }

    public void Exit()
    {
    }

    public void Dispose()
    {
    }
}