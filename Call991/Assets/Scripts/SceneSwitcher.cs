using System;
using System.Threading;
using System.Threading.Tasks;
using Configs;
using Data;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SceneSwitcher : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public ScenesHandler ScenesHandler;
        public VideoManager VideoManager;
        public Blocker Blocker;
        public GameSet GameSet;
        public CursorSet CursorSettings;
        public OverridenDialogue OverridenDialogue;
    }

    private Ctx _ctx;
    private CompositeDisposable _diposables;

    private IGameScene _currentScene;

    private CancellationTokenSource _tokenSource;
    public SceneSwitcher(Ctx ctx)
    {
        _ctx = ctx;
        _diposables = new CompositeDisposable();
        _tokenSource = new CancellationTokenSource().AddTo(_diposables);
        _ctx.OnSwitchScene.Subscribe(OnSwitchScene).AddTo(_diposables);
    }

    private void OnSwitchScene(GameScenes scene)
    {
        // load switch scene Additive (with UI over all)
        _diposables.Add(SceneManager.LoadSceneAsync(_ctx.ScenesHandler.SwitchScene) // async load scene
            .AsAsyncOperationObservable() // as Observable thread
            .Do(x =>
            {
                // call during the process
                Debug.Log($"[{this}][OnSwitchScene] Async load scene {_ctx.ScenesHandler.SwitchScene} progress: " +
                          x.progress); // show progress
            }).Subscribe(async _ =>
            {
                Debug.Log($"[{this}][OnSwitchScene] Async load scene {_ctx.ScenesHandler.SwitchScene} done");
                _currentScene?.Exit();
                _currentScene?.Dispose();
                await OnSwitchSceneLoaded(scene);
            }));
    }

    private async Task OnSwitchSceneLoaded(GameScenes scene)
    {
        _ctx.CursorSettings.EnableCursor(false);

        var onLoadingProcess = new ReactiveProperty<string>().AddTo(_diposables);
        var switchSceneEntity = _ctx.ScenesHandler.LoadingSceneEntity(onLoadingProcess, scene);

        var toLevelScene = scene == GameScenes.Level;

        if (toLevelScene)
        {
            _ctx.Blocker.EnableScreenFade(true);
            _ctx.VideoManager.EnableVideo(true);
        }

        Debug.Log($"[{this}][OnSwitchSceneLoaded] Start load scene {scene}");

        _currentScene = await _ctx.ScenesHandler.SceneEntity(scene);
        if (_tokenSource.IsCancellationRequested) return;

        SceneManager.LoadSceneAsync(_ctx.ScenesHandler.GetSceneName(scene)) // async load scene
            .AsAsyncOperationObservable() // as Observable thread
            .Do(x =>
            {
                // call during the process
                Debug.Log($"[{this}][OnSwitchSceneLoaded] Async load scene {scene} progress: " +
                          x.progress); // show progress
                onLoadingProcess.Value = x.progress.ToString();
            }).Subscribe(_ =>
            {
                Debug.Log($"[{this}][OnSwitchSceneLoaded] Async load scene {scene} done");
                switchSceneEntity.Exit();
                switchSceneEntity.Dispose();

                _currentScene.Enter();
            }).AddTo(_diposables);
    }

    public void Dispose()
    {
        _tokenSource.Cancel();
        _currentScene?.Dispose();
        _diposables.Dispose();
    }
}