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

        public ReactiveCommand<GameScenes> OnSwitchScene;
        public PlayerProfile Profile;
        public AudioManager AudioManager;
        public VideoManager VideoManager;
        public Blocker Blocker;
        public CursorSet CursorSettings;
        public ObjectEvents ObjectEvents;
        public OverridenDialogue OverridenDialogue;
        public ReactiveProperty<bool> IsPauseAllowed;
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
        // from prefab, or find, or addressable
        _ui = Object.FindObjectOfType<UiLevelScene>();

        var onClickMenuButton = new ReactiveCommand().AddTo(_disposables);

        var onShowPhrase = new ReactiveCommand<UiPhraseData>();
        var onHidePhrase = new ReactiveCommand<UiPhraseData>().AddTo(_disposables);
        var onAfterEnter = new ReactiveCommand().AddTo(_disposables);
        var onLevelEnd = new ReactiveCommand<List<RecordData>>().AddTo(_disposables);
        var onShowNewspaper = new ReactiveCommand<(Container<bool> btnPressed, Sprite sprite)>().AddTo(_disposables);
        var onShowLevelUi = new ReactiveCommand(); // on newspaper done
        var onSkipPhrase = new ReactiveCommand().AddTo(_disposables);
        var onClickPauseButton = new ReactiveCommand<bool>().AddTo(_disposables);

        var buttons = _ui.Buttons;
        var countDown = _ui.CountDown;
        var languages = _ctx.LevelData.GetEntryNode().Languages;

        var phraseSoundPlayer = new PhraseSoundPlayer(new PhraseSoundPlayer.Ctx
        {
            AudioSource = _ui.PhraseAudioSource,
        }).AddTo(_disposables);

        var contentLoader = new ContentLoader(new ContentLoader.Ctx
        {
            Languages = languages,
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
        }).AddTo(_disposables);

        var scenePm = new LevelScenePm(new LevelScenePm.Ctx
        {
            FindNext = findNext,
            OnNext = onNext,
            OverridenDialogue = _ctx.OverridenDialogue,
            onSwitchScene = _ctx.OnSwitchScene,
            onClickMenuButton = onClickMenuButton,
            OnShowLevelUi = onShowLevelUi,

            OnShowPhrase = onShowPhrase,
            PhraseSoundPlayer = phraseSoundPlayer,
            ContentLoader = contentLoader,
            ObjectEvents = _ctx.ObjectEvents,

            onHidePhrase = onHidePhrase,
            onAfterEnter = onAfterEnter,
            gameSet = _ctx.GameSet,
            buttons = buttons,
            countDown = countDown,
            AudioManager = _ctx.AudioManager,
            OnLevelEnd = onLevelEnd,
            OnShowNewspaper = onShowNewspaper,
            onSkipPhrase = onSkipPhrase,
            onClickPauseButton = onClickPauseButton,
            videoManager = _ctx.VideoManager,
            Blocker = _ctx.Blocker,
            cursorSettings = _ctx.CursorSettings,
        }).AddTo(_disposables);

        _ui.SetCtx(new UiLevelScene.Ctx
        {
            OnClickMenuButton = onClickMenuButton,
            OnShowPhrase = onShowPhrase,
            OnHidePhrase = onHidePhrase,
            OnShowTitle = _ctx.ObjectEvents.EventsGroup.OnShowTitle,
            OnShowWarning = _ctx.ObjectEvents.EventsGroup.OnShowWarning,
            OnLevelEnd = onLevelEnd,
            OnShowNewspaper = onShowNewspaper,
            OnShowLevelUi = onShowLevelUi,
            OnClickPauseButton = onClickPauseButton,
            AudioManager = _ctx.AudioManager,
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