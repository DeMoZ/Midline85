using System.Threading.Tasks;
using Configs;
using UI;
using UniRx;

public class MenuSceneEntity : IGameScene
{
    public struct Ctx
    {
        public Container<Task> ConstructorTask;
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public GameSet GameSet;
        public PlayerProfile Profile;
        public WwiseAudio AudioManager;
        public DialogueLoggerPm DialogueLogger;
    }

    private Ctx _ctx;
    private UiMenuScene _ui;

    private ReactiveCommand<int> _onLevelSelect;
    private ReactiveCommand<int> _onLevelPlay;
    private ReactiveCommand _onClickNewGame;

    public MenuSceneEntity(Ctx ctx)
    {
        _ctx = ctx;

        _onLevelPlay = new ReactiveCommand<int>();
        _onLevelSelect = new ReactiveCommand<int>();
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
            OnLevelPlay = _onLevelPlay,
            OnLevelSelect = _onLevelSelect,
            OnClickNewGame = _onClickNewGame,
            
            OnSwitchScene = _ctx.OnSwitchScene,
            Profile = _ctx.Profile,
        });
        
        _ui = UnityEngine.GameObject.FindObjectOfType<UiMenuScene>();

        _ui.SetCtx(new UiMenuScene.Ctx
        {
            OnLevelSelect = _onLevelSelect,
            OnLevelPlay = _onLevelPlay,
            OnClickNewGame = _onClickNewGame,
            Profile = _ctx.Profile,
            AudioManager = _ctx.AudioManager,
            GameSet = _ctx.GameSet,
            DialogueLogger = _ctx.DialogueLogger,
        });
    }

    public void Exit()
    {
    }

    public void Dispose()
    {
    }
}