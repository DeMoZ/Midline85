using System.Threading.Tasks;
using Configs;
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
        public Dialogues dialogues;
        public ReactiveCommand<GameScenes> onSwitchScene;
        public PlayerProfile profile;
        public string sceneVideoUrl;
        public string phraseSoundPath;
        public AudioManager audioManager;
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
        var onPhraseEvent = new ReactiveCommand<string>().AddTo(_disposables);
        var onShowPhrase = new ReactiveCommand<PhraseSet>().AddTo(_disposables);
        var onHidePhrase = new ReactiveCommand<PhraseSet>().AddTo(_disposables);
        var onShowIntro = new ReactiveCommand<bool>().AddTo(_disposables);

        var onAfterEnter = new ReactiveCommand().AddTo(_disposables);
        var buttons = _ui.Buttons;
        var countDown = _ui.CountDown;
        var videoPlayer = _ui.VideoPlayer;
        videoPlayer.url = _ctx.sceneVideoUrl;

        var phraseSoundPm = new PhraseSoundPlayer(new PhraseSoundPlayer.Ctx
        {
            path = _ctx.phraseSoundPath,
            audioSource = _ui.PhraseAudioSource,
        }).AddTo(_disposables);
        
        var scenePm = new LevelScenePm(new LevelScenePm.Ctx
        {
            profile = _ctx.profile,
            dialogues = _ctx.dialogues,
            onSwitchScene = _ctx.onSwitchScene,
            onClickMenuButton = onClickMenuButton,
            onPhraseEvent = onPhraseEvent,
            onShowPhrase = onShowPhrase,
            onHidePhrase = onHidePhrase,
            onAfterEnter = onAfterEnter,
            gameSet = _ctx.gameSet,
            buttons = buttons,
            countDown = countDown,
            phraseSoundPlayer = phraseSoundPm,
            audioManager = _ctx.audioManager,
            onShowIntro = onShowIntro,
        }).AddTo(_disposables);

        _ui.SetCtx(new UiLevelScene.Ctx
        {
            onClickMenuButton = onClickMenuButton,
            onPhraseEvent = onPhraseEvent,
            onShowPhrase = onShowPhrase,
            onHidePhrase = onHidePhrase,
            pool = uiPool,
            onShowIntro = onShowIntro,
        });

            // await BlackScreenFade();
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