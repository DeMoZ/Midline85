using System.Collections.Generic;
using System.Threading.Tasks;
using Configs;
using Core;
using Data;
using UI;
using UniRx;
using UnityEngine;

public class LevelSceneEntity : IGameScene
{
    public struct Ctx
    {
        public GameSet gameSet;
        public Container<Task> constructorTask;
        public ChapterSet chapterSet;
        public Dialogues dialogues;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public PlayerProfile profile;
        public AchievementsSo achievementsSo;
        public string phraseSoundPath;
        public string endLevelConfigsPath;
        public AudioManager audioManager;
        public VideoManager videoManager;
        public Sprite newspaperSprite;
        public PhraseEventVideoLoader phraseEventVideoLoader;
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
        await Task.Delay(10);
        await _ctx.audioManager.PlayMusic("7_lvl");
        // todo load from addressables black screen above the scene;
        // scene doesnt exist here
        // so just load and show on enter. Is it instant?
    }

    public async void Enter()
    {
        Debug.Log($"[{this}] Entered");
        // from prefab, or find, or addressable
        _ui = UnityEngine.GameObject.FindObjectOfType<UiLevelScene>();
        var uiPool = new Pool(new GameObject("uiPool").transform);

        var onClickMenuButton = new ReactiveCommand().AddTo(_disposables);
        var onPhraseSoundEvent = new ReactiveCommand<PhraseEvent>().AddTo(_disposables);
        var onShowPhrase = new ReactiveCommand<PhraseSet>().AddTo(_disposables);
        var onHidePhrase = new ReactiveCommand<PhraseSet>().AddTo(_disposables);
        var onShowIntro = new ReactiveCommand<bool>().AddTo(_disposables);
        var onAfterEnter = new ReactiveCommand().AddTo(_disposables);

        var onPopulateStatistics = new ReactiveCommand<List<StatisticElement>>();
        var onPhraseLevelEndEvent = new ReactiveCommand<string>();
        var onHideLevelUi = new ReactiveCommand<float>();
        var onShowStatisticUi = new ReactiveCommand<float>();
        var onShowNewspaper = new ReactiveCommand<(Container<Task> task, Sprite sprite)>();

        var buttons = _ui.Buttons;
        var countDown = _ui.CountDown;

        var phraseSoundPm = new PhraseSoundPlayer(new PhraseSoundPlayer.Ctx
        {
            path = _ctx.phraseSoundPath,
            audioSource = _ui.PhraseAudioSource,
        }).AddTo(_disposables);

        var phraseEventSoundLoader = new PhraseEventSoundLoader(new PhraseEventSoundLoader.Ctx
        {
            audioManager = _ctx.audioManager,
            eventSoPath = "PhraseSFX",
            streamingPath = "Sounds/EventSounds",
            resourcesPath = "Sounds/EventSounds",
        }).AddTo(_disposables);

        var levelEndPm = new LevelEndPm(new LevelEndPm.Ctx
        {
            gameSet = _ctx.gameSet,
            onHideLevelUi = onHideLevelUi,
            achievementsSo = _ctx.achievementsSo,
            profile = _ctx.profile,
            onShowStatisticUi = onShowStatisticUi,
            onPhraseLevelEndEvent = onPhraseLevelEndEvent,
            endLevelConfigsPath = _ctx.endLevelConfigsPath,
            phraseEventSoundLoader = phraseEventSoundLoader,
            phraseEventVideoLoader = _ctx.phraseEventVideoLoader,
            onPopulateStatistics = onPopulateStatistics,
        }).AddTo(_disposables);

        var scenePm = new LevelScenePm(new LevelScenePm.Ctx
        {
            profile = _ctx.profile,
            dialogues = _ctx.dialogues,
            onSwitchScene = _ctx.onSwitchScene,
            onClickMenuButton = onClickMenuButton,
            onPhraseSoundEvent = onPhraseSoundEvent,
            onShowPhrase = onShowPhrase,
            onHidePhrase = onHidePhrase,
            onAfterEnter = onAfterEnter,
            gameSet = _ctx.gameSet,
            buttons = buttons,
            countDown = countDown,
            phraseSoundPlayer = phraseSoundPm,
            phraseEventSoundLoader = phraseEventSoundLoader,
            audioManager = _ctx.audioManager,
            onShowIntro = onShowIntro,
            onPhraseLevelEndEvent = onPhraseLevelEndEvent,
            newspaperSprite = _ctx.newspaperSprite,
            onShowNewspaper = onShowNewspaper,
            chapterSet = _ctx.chapterSet,
            phraseEventVideoLoader = _ctx.phraseEventVideoLoader,
        }).AddTo(_disposables);

        _ui.SetCtx(new UiLevelScene.Ctx
        {
            onClickMenuButton = onClickMenuButton,
            onPhraseSoundEvent = onPhraseSoundEvent,
            onShowPhrase = onShowPhrase,
            onHidePhrase = onHidePhrase,
            onShowIntro = onShowIntro,
            onHideLevelUi = onHideLevelUi,
            onPopulateStatistics = onPopulateStatistics,
            onShowStatisticUi = onShowStatisticUi,
            onShowNewspaper = onShowNewspaper,
            pool = uiPool,
        });

        onAfterEnter.Execute();
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