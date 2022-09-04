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
        public Dialogues dialogues;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public PlayerProfile profile;
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
        Debug.Log($"[{this}] Entered");

        var gameSet = Resources.Load<GameSet>("GameSet");

        // from prefab, or find, or addressable
        _ui = UnityEngine.GameObject.FindObjectOfType<UiLevelScene>();
        //var camera = _ui.GetCamera();
        var uiPool = new Pool(new GameObject("uiPool").transform);

        var onClickMenuButton = new ReactiveCommand().AddTo(_disposables);
        var onPhraseEvent = new ReactiveCommand<string>().AddTo(_disposables);
        var onPhrase = new ReactiveCommand<Phrase>().AddTo(_disposables);

        var scenePm = new LevelScenePm(new LevelScenePm.Ctx
        {
            profile = _ctx.profile,
            dialogues = _ctx.dialogues,
            onSwitchScene = _ctx.onSwitchScene,
            onClickMenuButton = onClickMenuButton,
            onPhraseEvent = onPhraseEvent,
            onPhrase = onPhrase,
        }).AddTo(_disposables);


        _ui.SetCtx(new UiLevelScene.Ctx
        {
            onClickMenuButton = onClickMenuButton,
            onPhraseEvent = onPhraseEvent,
            onPhrase = onPhrase,
            gameSet = gameSet,
            pool = uiPool,
        });
    }

    public void Exit()
    {
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}