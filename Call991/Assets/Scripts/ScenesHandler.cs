using System;
using System.Threading.Tasks;
using Configs;
using Data;
using UniRx;

public class ScenesHandler : IDisposable
{
    public struct Ctx
    {
        public string startApplicationSceneName;
        public ReactiveCommand onStartApplicationSwitchScene;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public PlayerProfile profile;
        public AudioManager audioManager;
        public GameSet gameSet;
    }

    private const string ROOT_SCENE = "1_RootScene";
    private const string SWITCH_SCENE = "2_SwitchScene";
    private const string MENU_SCENE = "MenuScene";
    private const string LEVEL_SCENE = "LevelScene";

    private Ctx _ctx;
    private CompositeDisposable _disposables;

    public string RootScene => ROOT_SCENE;
    public string MenuScene => MENU_SCENE;
    public string SwitchScene => SWITCH_SCENE;
    public string LevelScene => LEVEL_SCENE;

    public ScenesHandler(Ctx ctx)
    {
        _ctx = ctx;
        _disposables = new CompositeDisposable();
        _ctx.onStartApplicationSwitchScene.Subscribe(_ => SelectSceneForStartApplication()).AddTo(_disposables);
    }


    private void SelectSceneForStartApplication()
    {
        switch (_ctx.startApplicationSceneName)
        {
            // case ROOT_SCENE:
            //     _ctx.onSwitchScene.Execute(GameScenes.Menu);
            //     break;
            // case MENU_SCENE:
            //     _ctx.onSwitchScene.Execute(GameScenes.Menu);
            //     break;
            // case SWITCH_SCENE:
            //     _ctx.onSwitchScene.Execute(GameScenes.Menu);
            //     break;
            case LEVEL_SCENE:
                _ctx.onSwitchScene.Execute(GameScenes.Level1);
                break;
            default:
                _ctx.onSwitchScene.Execute(GameScenes.Menu);
                break;
        }
    }

    public string GetSceneName(GameScenes scene)
    {
        return scene switch
        {
            GameScenes.Menu => MenuScene,
            GameScenes.Level1 => LevelScene,
            _ => throw new ArgumentOutOfRangeException(nameof(scene), scene, null)
        };
    }

    public async Task<IGameScene> SceneEntity(GameScenes scene)
    {
        IGameScene newScene = scene switch
        {
            GameScenes.Menu => await LoadMenu(),
            GameScenes.Level1 => await LoadLevel7(),
            _ => await LoadMenu()
        };

        return newScene;
    }

    private async Task<IGameScene> LoadMenu()
    {
        var constructorTask = new Container<Task>();
        var sceneEntity = new MenuSceneEntity(new MenuSceneEntity.Ctx
        {
            onSwitchScene = _ctx.onSwitchScene,
            profile = _ctx.profile,
            audioManager = _ctx.audioManager,
            constructorTask = constructorTask,
        });
        
        await constructorTask.Value;
        return sceneEntity;
    }

    private async Task<IGameScene> LoadLevel7()
    {
        var tLanguage = _ctx.profile.TextLanguage;
        var aLanguage = _ctx.profile.AudioLanguage;

        var compositeDialogue = await ResourcesLoader.LoadAsync<CompositeDialogue>("7_lvl_Total");
        var dialogues = await compositeDialogue.LoadDialogues(tLanguage, "7_lvl");
        var videoPathBuilder = new VideoPathBuilder();
        var sceneVideoUrl = videoPathBuilder.GetPath("VideoBack.mp4");

        //var phraseSoundPath = "Sounds/"+aLanguage+"/"+aLanguage+"_7_P";
        var phraseSoundPath = "Sounds/Ru/RU_7_P";
        
        var constructorTask = new Container<Task>();
        var sceneEntity = new LevelSceneEntity(new LevelSceneEntity.Ctx
        {
            gameSet = _ctx.gameSet,
            constructorTask = constructorTask,
            profile = _ctx.profile,
            dialogues = dialogues,
            onSwitchScene = _ctx.onSwitchScene,
            sceneVideoUrl = sceneVideoUrl,
            phraseSoundPath = phraseSoundPath,
            audioManager = _ctx.audioManager,
        });

        await constructorTask.Value;
        return sceneEntity;
    }

    public void Dispose()
    {
        ResourcesLoader.UnloadUnused();
        _disposables.Dispose();
    }
}