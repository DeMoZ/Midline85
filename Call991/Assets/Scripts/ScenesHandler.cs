using System;
using System.Collections.Generic;
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
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public PlayerProfile Profile;
        public WwiseAudio AudioManager;
        public GameSet GameSet;
        public VideoManager VideoManager;
        public Blocker Blocker;
        public CursorSet CursorSettings;
        public ObjectEvents ObjectEvents;
        public OverridenDialogue OverridenDialogue;
        public ReactiveProperty<bool> IsPauseAllowed;
        public ReactiveProperty<List<string>> LevelLanguages;
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
                _ctx.OnSwitchScene.Execute(GameScenes.OpenScene);
                break;
            // case MENU_SCENE:
            //     _ctx.onSwitchScene.Execute(GameScenes.Menu);
            //     break;
            // case SWITCH_SCENE:
            //     _ctx.onSwitchScene.Execute(GameScenes.Menu);
            //     break;

            case LEVEL_SCENE:
            case LEVEL_TEST_SCENE:
                _ctx.OnSwitchScene.Execute(GameScenes.Level);
                break;
            default:
                _ctx.OnSwitchScene.Execute(GameScenes.Menu);
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
            GameSet = _ctx.GameSet,
            OnSwitchScene = _ctx.OnSwitchScene,
            Blocker = _ctx.Blocker,
            CursorSettings = _ctx.CursorSettings,
        }).AddTo(_disposables);

        return sceneEntity;
    }

    private async Task<IGameScene> LoadMenu()
    {
        _ctx.AudioManager.OnSceneSwitch();

        var constructorTask = new Container<Task>();
        var sceneEntity = new MenuSceneEntity(new MenuSceneEntity.Ctx
        {
            OnSwitchScene = _ctx.OnSwitchScene,
            Profile = _ctx.Profile,
            AudioManager = _ctx.AudioManager,
            videoManager = _ctx.VideoManager,
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
            ConstructorTask = constructorTask,
            LevelData = levelData,
            Profile = _ctx.Profile,
            ObjectEvents = _ctx.ObjectEvents,
            OnSwitchScene = _ctx.OnSwitchScene,
            AudioManager = _ctx.AudioManager,
            VideoManager = _ctx.VideoManager,
            OverridenDialogue = _ctx.OverridenDialogue,
            Blocker = _ctx.Blocker,
            CursorSettings = _ctx.CursorSettings,
            IsPauseAllowed = _ctx.IsPauseAllowed,
            LevelLanguages = _ctx.LevelLanguages,
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
        _ctx.VideoManager.EnableVideo(false);

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

    public async Task<LoadingSceneEntity> CreateLoadingSceneEntity(bool toLevelScene, ReactiveProperty<string> onLoadingProcess)
    {
        await Task.Yield();
        return new LoadingSceneEntity(new LoadingSceneEntity.Ctx
        {
            OnLoadingProcess = onLoadingProcess, // onLoadingProcess, TODO possible need a number.ToString()
            ToLevelScene = toLevelScene,
            FirstLoad = toLevelScene,
            Blocker = _ctx.Blocker,
            GameSet = _ctx.GameSet,
        }).AddTo(_disposables);
    }
}