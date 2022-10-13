using System;
using System.Threading.Tasks;
using Configs;
using Data;
using UniRx;
using UnityEngine;

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
        public VideoManager videoManager;
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
        _ctx.onStartApplicationSwitchScene.Subscribe(_ => SelectSceneForStartApplication()).AddTo(_disposables);
    }


    private void SelectSceneForStartApplication()
    {
        switch (_ctx.startApplicationSceneName)
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

        return newScene;
    }

    private IGameScene LoadOpenScene()
    {
        var sceneEntity = new OpenSceneEntity(new OpenSceneEntity.Ctx
        {
            gameSet = _ctx.gameSet,
            onSwitchScene = _ctx.onSwitchScene,
        }).AddTo(_disposables);

        return sceneEntity;
    }

    private async Task<IGameScene> LoadMenu()
    {
        _ctx.audioManager.OnSceneSwitch();
        _ctx.videoManager.Enable(false);

        var constructorTask = new Container<Task>();
        var sceneEntity = new MenuSceneEntity(new MenuSceneEntity.Ctx
        {
            onSwitchScene = _ctx.onSwitchScene,
            profile = _ctx.profile,
            audioManager = _ctx.audioManager,
            videoManager = _ctx.videoManager,
            constructorTask = constructorTask,
        }).AddTo(_disposables);
        
        await constructorTask.Value;
        return sceneEntity;
    }

    private async Task<IGameScene> LoadLevel7()
    {
        var tLanguage = _ctx.profile.TextLanguage;
        var aLanguage = _ctx.profile.AudioLanguage;
        var levelSoFolder = "7_lvl";
        var chapterSet = await ResourcesLoader.LoadAsync<ChapterSet>(levelSoFolder + "/7_lvl_Total");
        var dialogues = await chapterSet.LoadDialogues(tLanguage, levelSoFolder);
        var videoPathBuilder = new VideoPathBuilder();
        //var phraseSoundPath = "Sounds/"+aLanguage+"/"+aLanguage+"_7_P";
        var phraseSoundPath = "Sounds/Ru/RU_7_P";
        var achievementsSo = await ResourcesLoader.LoadAsync<AchievementsSo>(levelSoFolder + "/7_lvl_achievements");
        var endLevelConfigsPath = levelSoFolder;
        var newspaperSprite =  await ResourcesLoader.LoadAsync<Sprite>(levelSoFolder + "/newspaper");
        _ctx.audioManager.OnSceneSwitch();
        _ctx.videoManager.Enable(true);

        var phraseEventVideoLoader = new PhraseEventVideoLoader(new PhraseEventVideoLoader.Ctx
        {
            eventSoPath = levelSoFolder,
            videoManager = _ctx.videoManager,
            streamingPath = "Videos/EventVideos",
        }).AddTo(_disposables);
        
        var constructorTask = new Container<Task>();
        var sceneEntity = new LevelSceneEntity(new LevelSceneEntity.Ctx
        {
            gameSet = _ctx.gameSet,
            constructorTask = constructorTask,
            profile = _ctx.profile,
            chapterSet = chapterSet,
            dialogues = dialogues,
            onSwitchScene = _ctx.onSwitchScene,
            achievementsSo = achievementsSo,
            phraseSoundPath = phraseSoundPath,
            endLevelConfigsPath = endLevelConfigsPath,
            audioManager = _ctx.audioManager,
            videoManager = _ctx.videoManager,
            newspaperSprite = newspaperSprite,
            phraseEventVideoLoader = phraseEventVideoLoader,
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
        var toLevelScene = scene == GameScenes.Level1;
        _ctx.videoManager.Enable(toLevelScene);
        
        var phraseEventVideoLoader = new PhraseEventVideoLoader(new PhraseEventVideoLoader.Ctx
        {
            videoManager = _ctx.videoManager,
            streamingPath = "Videos/EventVideos",
        }).AddTo(_disposables);
        
        var switchSceneEntity = new LoadingSceneEntity(new LoadingSceneEntity.Ctx
        {
            onLoadingProcess = onLoadingProcess,
            toLevelScene = toLevelScene,
            phraseEventVideoLoader = phraseEventVideoLoader,
            gameSet = _ctx.gameSet,
        }).AddTo(_disposables);

        return switchSceneEntity;
    }
}