using Configs;
using UI;
using UniRx;

public class LoadingSceneEntity : IGameScene
{
    public struct Ctx
    {
        public ReactiveProperty<string> OnLoadingProcess;
        public bool ToLevelScene;
        public bool FirstLoad;
        public Blocker Blocker;
        public GameSet GameSet;
    }

    private Ctx _ctx;

    public LoadingSceneEntity(Ctx ctx)
    {
        _ctx = ctx;

        var ui = UnityEngine.Object.FindObjectOfType<UiSwitchScene>();
        ui.SetCtx(new UiSwitchScene.Ctx
        {
            onLoadingProcess = _ctx.OnLoadingProcess,
            toLevelScene = _ctx.ToLevelScene,
            firstLoad = _ctx.FirstLoad,
            blocker = _ctx.Blocker,
            gameSet = _ctx.GameSet,
        });
    }

    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public void Dispose()
    {
    }
}