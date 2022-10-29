using Configs;
using Data;
using UI;
using UniRx;

public class LoadingSceneEntity : IGameScene
{
    public struct Ctx
    {
        public ReactiveProperty<string> onLoadingProcess;
        public bool toLevelScene;
        public bool firstLoad;
        public PhraseEventVideoLoader phraseEventVideoLoader;
        public GameSet gameSet;
    }

    private Ctx _ctx;

    public LoadingSceneEntity(Ctx ctx)
    {
        _ctx = ctx;

        var ui = UnityEngine.GameObject.FindObjectOfType<UiSwitchScene>();
        ui.SetCtx(new UiSwitchScene.Ctx
        {
            onLoadingProcess = _ctx.onLoadingProcess,
            toLevelScene = _ctx.toLevelScene,
            firstLoad = _ctx.firstLoad,
            phraseEventVideoLoader = _ctx.phraseEventVideoLoader,
            gameSet = _ctx.gameSet,
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