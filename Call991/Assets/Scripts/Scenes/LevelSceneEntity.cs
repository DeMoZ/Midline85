using System.Collections.Generic;
using System.Threading.Tasks;
using AaDialogueGraph;
using Configs;
using Data;
using UI;
using UniRx;
using UnityEngine;

public class LevelSceneEntity : IGameScene
{
    public struct Ctx
    {
        public GameSet GameSet;
        public Container<Task> ConstructorTask;
        public LevelData LevelData;

        public ReactiveProperty<List<string>> LevelLanguages;
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public PlayerProfile Profile;
        public MediaService MediaService;
        public Blocker Blocker;
        public CursorSet CursorSettings;
        public ObjectEvents ObjectEvents;
        public OverridenDialogue OverridenDialogue;
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

        var onShowPhrase = new ReactiveCommand<UiPhraseData>();
        var onShowImagePhrase = new ReactiveCommand<UiImagePhraseData>();
        var onHidePhrase = new ReactiveCommand<UiPhraseData>().AddTo(_disposables);
        var onHideImagePhrase = new ReactiveCommand<UiImagePhraseData>().AddTo(_disposables);
        
        var onAfterEnter = new ReactiveCommand().AddTo(_disposables);
        var onLevelEnd = new ReactiveCommand<List<RecordData>>().AddTo(_disposables);
        var onShowNewspaper = new ReactiveCommand<(Container<bool> btnPressed, Sprite sprite)>().AddTo(_disposables);
        var onShowLevelUi = new ReactiveCommand(); // on newspaper done
        var onSkipPhrase = new ReactiveCommand().AddTo(_disposables);
        var onClickPauseButton = new ReactiveCommand<bool>().AddTo(_disposables);

        var buttons = _ui.Buttons;
        var countDown = _ui.CountDown;
        _ctx.LevelLanguages.Value = _ctx.LevelData.GetEntryNode().Languages;

        var contentLoader = new ContentLoader(new ContentLoader.Ctx
        {
            LevelLanguages = _ctx.LevelLanguages.Value,
            Profile = _ctx.Profile,
        }).AddTo(_disposables);

        var phraseSkipper = new PhraseSkipper(onSkipPhrase).AddTo(_disposables);

        var onNext = new ReactiveCommand<List<AaNodeData>>().AddTo(_disposables);
        var findNext = new ReactiveCommand<List<AaNodeData>>().AddTo(_disposables);
        var dialoguePm = new DialoguePm(new DialoguePm.Ctx
        {
            LevelData = _ctx.LevelData,
            FindNext = findNext,
            OnNext = onNext,
            DialogueLogger = _ctx.DialogueLogger,
        }).AddTo(_disposables);

        var levelId = _ctx.LevelData.GetEntryNode().LevelId;
        var scenePm = new LevelScenePm(new LevelScenePm.Ctx
        {
            FindNext = findNext,
            OnNext = onNext,
            OverridenDialogue = _ctx.OverridenDialogue,
            OnSwitchScene = _ctx.OnSwitchScene,
            OnClickMenuButton = onClickMenuButton,
            OnShowLevelUi = onShowLevelUi,

            OnShowPhrase = onShowPhrase,
            OnShowImagePhrase = onShowImagePhrase,
            OnHidePhrase = onHidePhrase,
            OnHideImagePhrase = onHideImagePhrase,
            
            ContentLoader = contentLoader,
            ObjectEvents = _ctx.ObjectEvents,
            
            OnAfterEnter = onAfterEnter,
            GameSet = _ctx.GameSet,
            LevelId = levelId,
            buttons = buttons,
            CountDown = countDown,
            MediaService = _ctx.MediaService,
            OnLevelEnd = onLevelEnd,
            OnShowNewspaper = onShowNewspaper,
            OnSkipPhrase = onSkipPhrase,
            OnClickPauseButton = onClickPauseButton,
            Blocker = _ctx.Blocker,
            CursorSettings = _ctx.CursorSettings,
        }).AddTo(_disposables);

        _ui.SetCtx(new UiLevelScene.Ctx
        {
            OnClickMenuButton = onClickMenuButton,
            OnShowPhrase = onShowPhrase,
            OnShowImagePhrase = onShowImagePhrase,
            OnHidePhrase = onHidePhrase,
            OnHideImagePhrase = onHideImagePhrase,
            OnShowTitle = _ctx.ObjectEvents.EventsGroup.OnShowTitle,
            OnShowWarning = _ctx.ObjectEvents.EventsGroup.OnShowWarning,
            OnLevelEnd = onLevelEnd,
            OnShowNewspaper = onShowNewspaper,
            OnShowLevelUi = onShowLevelUi,
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