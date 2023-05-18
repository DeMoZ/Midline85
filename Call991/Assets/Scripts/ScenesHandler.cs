using System;
using System.Threading.Tasks;
using Configs;
using UI;
using UniRx;
using UnityEngine;

public class ScenesHandler : IDisposable
{
    public struct Ctx
    {
        public string StartApplicationSceneName;
        public ReactiveCommand OnStartApplicationSwitchScene;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public PlayerProfile Profile;
        public AudioManager AudioManager;
        public GameSet GameSet;
        public VideoManager videoManager;
        public Blocker Blocker;
        public CursorSet CursorSettings;
        public ObjectEvents ObjectEvents;
        public OverridenDialogue OverridenDialogue;
    }

    private const string ROOT_SCENE = "1_RootScene";
    private const string SWITCH_SCENE = "2_SwitchScene";
    private const string MENU_SCENE = "MenuScene";
    private const string LEVEL_SCENE = "LevelScene";
    private const string LEVEL_TEST_SCENE = "LevelTestScene";
    private const string OPEN_SCENE = "OpenScene";

    private Ctx _ctx;
    private CompositeDisposable _disposables;

    public string RootScene => ROOT_SCENE;
    public string MenuScene => MENU_SCENE;
    public string SwitchScene => SWITCH_SCENE;
    public string LevelScene => LEVEL_SCENE;
    public string LevelTestScene => LEVEL_TEST_SCENE;
    public string OpenScene => OPEN_SCENE;

    public ScenesHandler(Ctx ctx)
    {
        _ctx = ctx;
        _disposables = new CompositeDisposable();
        _ctx.OnStartApplicationSwitchScene.Subscribe(_ => SelectSceneForStartApplication()).AddTo(_disposables);
        _ctx.CursorSettings.ApplyCursor();
    }


    private void SelectSceneForStartApplication()
    {
        switch (_ctx.StartApplicationSceneName)
        {
            case ROOT_SCENE:
                _ctx.onSwitchScene.Execute(GameScenes.OpenScene);
                break;
            // case MENU_SCENE:
            //     _ctx.onSwitchScene.Execute(GameScenes.Menu);
            //     break;
            // case SWITCH_SCENE:
            //     _ctx.onSwitchScene.Execute(GameScenes.Menu);
            //     break;

            case LEVEL_SCENE:
            case LEVEL_TEST_SCENE:
                _ctx.onSwitchScene.Execute(GameScenes.Level);
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
            GameScenes.OpenScene => OpenScene,
            GameScenes.Menu => MenuScene,
            GameScenes.Level => LevelScene,
            _ => throw new ArgumentOutOfRangeException(nameof(scene), scene, null)
        };
    }

    public async Task<IGameScene> SceneEntity(GameScenes scene)
    {
        IGameScene newScene = scene switch
        {
            GameScenes.OpenScene => LoadOpenScene(),
            GameScenes.Menu => await LoadMenu(),
            GameScenes.Level => await LoadLevel(),
            _ => await LoadMenu()
        };

        Time.timeScale = 1;
        return newScene;
    }

    private IGameScene LoadOpenScene()
    {
        var sceneEntity = new OpenSceneEntity(new OpenSceneEntity.Ctx
        {
            gameSet = _ctx.GameSet,
            onSwitchScene = _ctx.onSwitchScene,
            blocker = _ctx.Blocker,
            cursorSettings = _ctx.CursorSettings,
        }).AddTo(_disposables);

        return sceneEntity;
    }

    private async Task<IGameScene> LoadMenu()
    {
        _ctx.AudioManager.OnSceneSwitch();

        var constructorTask = new Container<Task>();
        var sceneEntity = new MenuSceneEntity(new MenuSceneEntity.Ctx
        {
            OnSwitchScene = _ctx.onSwitchScene,
            Profile = _ctx.Profile,
            AudioManager = _ctx.AudioManager,
            videoManager = _ctx.videoManager,
            ConstructorTask = constructorTask,
        }).AddTo(_disposables);

        _ctx.CursorSettings.EnableCursor(true);
        await constructorTask.Value;
        return sceneEntity;
    }

    private async Task<IGameScene> LoadLevel()
    {
        var level = _ctx.OverridenDialogue.Dialogue != null
            ? _ctx.OverridenDialogue.Dialogue
            : _ctx.GameSet.GameLevels.TestLevel;

        var levelData = new LevelData(level.GetNodesData(), level.NodeLinks);
        
        _ctx.AudioManager.OnSceneSwitch();

        var constructorTask = new Container<Task>();
        var sceneEntity = new LevelSceneEntity(new LevelSceneEntity.Ctx
        {
            GameSet = _ctx.GameSet,
            constructorTask = constructorTask,
            LevelData = levelData,
            Profile = _ctx.Profile,
            ObjectEvents = _ctx.ObjectEvents,
            onSwitchScene = _ctx.onSwitchScene,
            AudioManager = _ctx.AudioManager,
            videoManager = _ctx.videoManager,
            OverridenDialogue = _ctx.OverridenDialogue,
            Blocker = _ctx.Blocker,
            cursorSettings = _ctx.CursorSettings,
        }).AddTo(_disposables);

        await constructorTask.Value;
        return sceneEntity;
    }

    public void Dispose()
    {
        ResourcesLoader.UnloadUnused();
        _disposables.Dispose();
    }

    public IGameScene LoadingSceneEntity(ReactiveProperty<string> onLoadingProcess, GameScenes scene)
    {
        _ctx.videoManager.EnableVideo(false);

        var toLevelScene = scene == GameScenes.Level;

        var switchSceneEntity = new LoadingSceneEntity(new LoadingSceneEntity.Ctx
        {
            OnLoadingProcess = onLoadingProcess,
            ToLevelScene = toLevelScene,
            FirstLoad = scene == GameScenes.OpenScene,
            Blocker = _ctx.Blocker,
            GameSet = _ctx.GameSet,
        }).AddTo(_disposables);

        return switchSceneEntity;
    }
}