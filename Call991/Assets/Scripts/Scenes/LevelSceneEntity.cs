using System.Collections.Generic;
using System.Threading.Tasks;
using Configs;
using Data;
using UI;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

public class LevelSceneEntity : IGameScene
{
    public struct Ctx
    {
        public GameSet GameSet;
        public Container<Task> ConstructorTask;

        public ReactiveProperty<List<string>> LevelLanguages;
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public PlayerProfile Profile;
        public GameLevelsService GameLevelsService;
        public MediaService MediaService;
        public Blocker Blocker;
        public CursorSet CursorSettings;
        public ObjectEvents ObjectEvents;
        public ReactiveProperty<bool> IsPauseAllowed;
        public DialogueLoggerPm DialogueLogger;
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
        _ctx.ConstructorTask.Value = ConstructorTask();
    }

    private async Task ConstructorTask()
    {
        await Task.Delay(10);

        // scene doesnt exist here
        // so just load and show on enter. Is it instant?
    }

    public async void Enter()
    {
        Debug.Log($"[{this}] Entered");

        _ui = Object.FindObjectOfType<UiLevelScene>();

        var onClickMenuButton = new ReactiveCommand().AddTo(_disposables);
        var onClickNextLevelButton = new ReactiveCommand().AddTo(_disposables);

        var dialogueService = new DialogueService().AddTo(_disposables);

        var onAfterEnter = new ReactiveCommand().AddTo(_disposables);
        var onLevelEnd = new ReactiveCommand<StatisticsData>().AddTo(_disposables);
        var onClickPauseButton = new ReactiveCommand<bool>().AddTo(_disposables);

        var levelSceneObjectsService = new LevelSceneObjectsService().AddTo(_disposables);
        _ctx.GameLevelsService.InitDialogue();
        var levelData = _ctx.GameLevelsService.LevelData;
        _ctx.LevelLanguages.Value = levelData.GetEntryNode().Languages;

        var contentLoader = new ContentLoader(new ContentLoader.Ctx
        {
            LevelLanguages = _ctx.LevelLanguages.Value,
            Profile = _ctx.Profile,
        }).AddTo(_disposables);

        var phraseSkipper = new PhraseSkipper(dialogueService.OnSkipPhrase).AddTo(_disposables);
        
        var levelId = levelData.GetEntryNode().LevelId;
        var scenePm = new LevelScenePm(new LevelScenePm.Ctx
        {
            OnSwitchScene = _ctx.OnSwitchScene,
            OnClickMenuButton = onClickMenuButton,
            OnClickNextLevelButton = onClickNextLevelButton,

            ContentLoader = contentLoader,
            ObjectEvents = _ctx.ObjectEvents,

            OnAfterEnter = onAfterEnter,
            GameSet = _ctx.GameSet,
            LevelId = levelId,
            LevelSceneObjectsService = levelSceneObjectsService,
            DialogueService = dialogueService,
            GameLevelsService = _ctx.GameLevelsService,
            MediaService = _ctx.MediaService,
            OnLevelEnd = onLevelEnd,
            OnClickPauseButton = onClickPauseButton,
            Blocker = _ctx.Blocker,
            CursorSettings = _ctx.CursorSettings,
        }).AddTo(_disposables);

        _ui.SetCtx(new UiLevelScene.Ctx
        {
            GameSet = _ctx.GameSet,
            LevelSceneObjectsService = levelSceneObjectsService,
            OnClickMenuButton = onClickMenuButton,
            OnClickNextLevelButton = onClickNextLevelButton,
            DialogueService = dialogueService,
            OnShowTitle = _ctx.ObjectEvents.EventsGroup.OnShowTitle,
            OnShowWarning = _ctx.ObjectEvents.EventsGroup.OnShowWarning,
            OnLevelEnd = onLevelEnd,
            OnClickPauseButton = onClickPauseButton,
            Profile = _ctx.Profile,
            IsPauseAllowed = _ctx.IsPauseAllowed,
        });

        onAfterEnter.Execute();
        await Task.Yield();
    }

    public void Exit()
    {
    }

    public void Dispose()
    {
        Resources.UnloadUnusedAssets();
        _disposables.Dispose();
    }
}