using System;
using System.IO;
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
    }

    private const string ROOT_SCENE = "1_RootScene";
    private const string SWITCH_SCENE = "2_SwitchScene";
    private const string MENU_SCENE = "MenuScene";
    private const string LEVEL_SCENE = "LevelScene";
    private const string OPEN_SCENE = "OpenScene";

    private Ctx _ctx;
    private CompositeDisposable _disposables;

    public string RootScene => ROOT_SCENE;
    public string MenuScene => MENU_SCENE;
    public string SwitchScene => SWITCH_SCENE;
    public string LevelScene => LEVEL_SCENE;
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
            GameScenes.OpenScene => OpenScene,
            GameScenes.Menu => MenuScene,
            GameScenes.Level1 => LevelScene,
            _ => throw new ArgumentOutOfRangeException(nameof(scene), scene, null)
        };
    }

    public async Task<IGameScene> SceneEntity(GameScenes scene)
    {
        IGameScene newScene = scene switch
        {
            GameScenes.OpenScene => LoadOpenScene(),
            GameScenes.Menu => await LoadMenu(),
            GameScenes.Level1 => await LoadLevel7(),
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

    private async Task<IGameScene> LoadLevel7()
    {
        var tLanguage = _ctx.Profile.TextLanguage;
        var levelData = new LevelData(_ctx.GameSet.GameLevels.TestLevel.GetNodesData(),
            _ctx.GameSet.GameLevels.TestLevel.NodeLinks);
        var levelFolder = "7_lvl";
        var chapterSet = await ResourcesLoader.LoadAsync<ChapterSet>(levelFolder + "/7_lvl_Total");
        var newspaperPath = Path.Combine(tLanguage.ToString(), levelFolder, "newspaper");
        var newspaperSprite = await ResourcesLoader.LoadAsync<Sprite>(newspaperPath);
        _ctx.AudioManager.OnSceneSwitch();
        
        var constructorTask = new Container<Task>();
        var sceneEntity = new LevelSceneEntity(new LevelSceneEntity.Ctx
        {
            GameSet = _ctx.GameSet,
            constructorTask = constructorTask,
            LevelData = levelData,
            Profile = _ctx.Profile,
            ObjectEvents = _ctx.ObjectEvents,
            chapterSet = chapterSet,
            onSwitchScene = _ctx.onSwitchScene,
            AudioManager = _ctx.AudioManager,
            videoManager = _ctx.videoManager,
            newspaperSprite = newspaperSprite,
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

        var toLevelScene = scene == GameScenes.Level1;

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