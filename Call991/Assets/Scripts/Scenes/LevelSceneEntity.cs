using System.Threading.Tasks;
using Configs;
using UI;
using UniRx;
using UnityEngine;

public class LevelSceneEntity : IGameScene
{
    public struct Ctx
    {
        public Container<Task> constructorTask;
        public ReactiveCommand<GameScenes> onSwitchScene;
    }

    private Ctx _ctx;
    private UiLevelScene _ui;
    private CompositeDisposable _disposables;

    public LevelSceneEntity(Ctx ctx)
    {
        _ctx = ctx;
        _disposables = new CompositeDisposable();

        AsyncConstructor();
    }

    private void AsyncConstructor()
    {
        _ctx.constructorTask.Value = ConstructorTask();
    }

    private async Task ConstructorTask()
    {
        // await Task.Yield();
        await Task.Delay(1 * 1000);
    }

    public void Enter()
    {
        var gameSet = Resources.Load<GameSet>("GameSet");

        // from prefab, or find, or addressable
        var camera = UnityEngine.GameObject.FindObjectOfType<Camera>();
        _ui = UnityEngine.GameObject.FindObjectOfType<UiLevelScene>();
        var uiPool = new Pool(new GameObject("uiPool").transform);

        var onClickMenuButton = new ReactiveCommand().AddTo(_disposables);
        
        var scenePm = new LevelScenePm(new LevelScenePm.Ctx
        {
            onSwitchScene = _ctx.onSwitchScene,
            onClickMenuButton = onClickMenuButton,
        }).AddTo(_disposables);


        _ui.SetCtx(new UiLevelScene.Ctx
        {
            onClickMenuButton = onClickMenuButton,
            gameSet = gameSet,
            pool = uiPool,
        });

        Debug.Log("[LevelSceneEntity] Entered");
    }

    public void Exit()
    {
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}