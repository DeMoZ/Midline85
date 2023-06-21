using System.Threading.Tasks;
using UI;
using UniRx;

public class MenuSceneEntity : IGameScene
{
    public struct Ctx
    {
        public Container<Task> constructorTask;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public PlayerProfile profile;
        public AudioManager audioManager;
        public VideoManager videoManager;
    }

    private Ctx _ctx;
    private UiMenuScene _ui;

    private ReactiveCommand _onClickPlayGame;
    private ReactiveCommand _onClickNewGame;

    public MenuSceneEntity(Ctx ctx)
    {
        _ctx = ctx;

        _onClickPlayGame = new ReactiveCommand();
        _onClickNewGame = new ReactiveCommand();
        AsyncConstructor();
    }

    private void AsyncConstructor()
    {
        _ctx.constructorTask.Value = ConstructorTask();
    }

    private async Task ConstructorTask()
    {
        // await Task.Yield();
        await Task.Delay(10);
        await _ctx.audioManager.PlayMusic("Intro");
        // todo load from addressables black screen above the scene;
        // scene doesnt exist here
        // so just load and show on enter. Is it instant?
    }

    public void Enter()
    {
        var menuScenePm = new MenuScenePm(new MenuScenePm.Ctx
        {
            onClickPlayGame = _onClickPlayGame,
            onClickNewGame = _onClickNewGame,
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
            profile = _ctx.profile,
            audioManager = _ctx.audioManager,
        });
    }

    public void Exit()
    {
    }

    public void Dispose()
    {
    }
}