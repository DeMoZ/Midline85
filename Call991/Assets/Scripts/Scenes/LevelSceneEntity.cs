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
        public Container<Task> constructorTask;
        public LevelData LevelData;

        public ChapterSet chapterSet;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public PlayerProfile Profile;
        public AudioManager AudioManager;
        public VideoManager videoManager;
        public Sprite newspaperSprite;
        public Blocker Blocker;
        public CursorSet cursorSettings;
        public ObjectEvents ObjectEvents;
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
        await Task.Delay(10);
        // todo should be removed and music must be played in other way
        await _ctx.AudioManager.PlayMusic("7_lvl");
        
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
        var onShowIntro = new ReactiveCommand<bool>().AddTo(_disposables);
        var onAfterEnter = new ReactiveCommand().AddTo(_disposables);

        var onLevelEnd = new ReactiveCommand<List<RecordData>>().AddTo(_disposables);
        var onShowNewspaper = new ReactiveCommand<(Container<Task> task, Sprite sprite)>().AddTo(_disposables);
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
        
        // _ctx.audioManager.SetPhraseAudioSource(_ui.PhraseAudioSource);

        var phraseSkipper = new PhraseSkipper(onSkipPhrase).AddTo(_disposables);

        var onNext = new ReactiveCommand<List<AaNodeData>>();
        var findNext = new ReactiveCommand<List<AaNodeData>>();
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
            Profile = _ctx.Profile,
            onSwitchScene = _ctx.onSwitchScene,
            onClickMenuButton = onClickMenuButton,

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
            onShowIntro = onShowIntro,
            OnLevelEnd = onLevelEnd,
            newspaperSprite = _ctx.newspaperSprite,
            onShowNewspaper = onShowNewspaper,
            chapterSet = _ctx.chapterSet,
            onSkipPhrase = onSkipPhrase,
            onClickPauseButton = onClickPauseButton,
            videoManager = _ctx.videoManager,
            Blocker = _ctx.Blocker,
            cursorSettings = _ctx.cursorSettings,
        }).AddTo(_disposables);

        _ui.SetCtx(new UiLevelScene.Ctx
        {
            onClickMenuButton = onClickMenuButton,
            OnShowPhrase = onShowPhrase,
            onHidePhrase = onHidePhrase,
            onShowIntro = onShowIntro,
            OnLevelEnd = onLevelEnd,
            onShowNewspaper = onShowNewspaper,
            onClickPauseButton = onClickPauseButton,
            AudioManager = _ctx.AudioManager,
            Profile = _ctx.Profile,
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