using System.Threading.Tasks;
using UI;
using UniRx;

public class MenuSceneEntity : IGameScene
{
    public struct Ctx
    {
        public Container<Task> ConstructorTask;
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public PlayerProfile Profile;
        public WwiseAudio AudioManager;
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
        _ctx.ConstructorTask.Value = ConstructorTask();
    }

    private async Task ConstructorTask()
    {
        // await Task.Yield();
        await Task.Delay(10);
        // TODO Wwise await _ctx.AudioManager.PlayMusic("Intro");
       
        // scene doesnt exist here
        // so just load and show on enter. Is it instant?
    }

    public void Enter()
    {
        var menuScenePm = new MenuScenePm(new MenuScenePm.Ctx
        {
            OnClickPlayGame = _onClickPlayGame,
            OnClickNewGame = _onClickNewGame,
            OnSwitchScene = _ctx.OnSwitchScene,
            Profile = _ctx.Profile,
        });
        
        // Find UI or instantiate from Addressable
        // _ui = Addressable.Instantiate();
        _ui = UnityEngine.GameObject.FindObjectOfType<UiMenuScene>();
        
        _ui.SetCtx(new UiMenuScene.Ctx
        {
            OnClickPlayGame = _onClickPlayGame,
            OnClickNewGame = _onClickNewGame,
            Profile = _ctx.Profile,
            AudioManager = _ctx.AudioManager,
        });
    }

    public void Exit()
    {
    }

    public void Dispose()
    {
    }
}