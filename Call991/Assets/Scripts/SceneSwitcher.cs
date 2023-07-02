using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Configs;
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
        public GameSet GameSet;
        public VideoManager VideoManager;
        public Blocker Blocker;
        public CursorSet CursorSettings;
    }

    private Ctx _ctx;
    private CompositeDisposable _disposables;

    private IGameScene _currentScene;

    private CancellationTokenSource _tokenSource;
    AsyncOperation _asyncLoad;

    public SceneSwitcher(Ctx ctx)
    {
        Debug.LogWarning($"{this} created");
        _ctx = ctx;
        _disposables = new CompositeDisposable();
        _tokenSource = new CancellationTokenSource().AddTo(_disposables);
        _ctx.OnSwitchScene.Subscribe(OnSwitchScene).AddTo(_disposables);
    }

    private async void OnSwitchScene(GameScenes scene)
    {
        if (_asyncLoad is { isDone: false })
        {
            Debug.LogWarning($"<color=red>[{this}]</color>Trying to load scene while loading is in process " +
                             $"already. <color=red>Aborted</color>");
            return;
        }

        // load switch scene
        _asyncLoad = SceneManager.LoadSceneAsync(_ctx.ScenesHandler.SwitchScene);
        var onLoadingProcess= new ReactiveProperty<string>("0");

        while (!_asyncLoad.isDone)
        {
            await Task.Yield();
            if (_tokenSource.IsCancellationRequested) return;
        }

        var loadingSceneEntity = await _ctx.ScenesHandler
            .CreateLoadingSceneEntity(scene == GameScenes.Level, onLoadingProcess);
        
        if (_tokenSource.IsCancellationRequested) return;

        // destroy previous scene
        _currentScene?.Exit();
        _currentScene?.Dispose();
     
        // load real scene after
        _asyncLoad = SceneManager.LoadSceneAsync(_ctx.ScenesHandler.GetSceneName(scene));

        while (!_asyncLoad.isDone)
        {
            await Task.Yield();
            onLoadingProcess.Value = _asyncLoad.progress.ToString(CultureInfo.InvariantCulture);
            
            if (_tokenSource.IsCancellationRequested) return;
        }

        // initialise loaded scene
        _currentScene = await _ctx.ScenesHandler.SceneEntity(scene).AddTo(_disposables);
        if (_tokenSource.IsCancellationRequested) return;

        // destroy switch scene
        loadingSceneEntity.Exit();
        loadingSceneEntity.Dispose();
        onLoadingProcess.Dispose();
        _currentScene.Enter();
    }

    // private void _OnSwitchScene(GameScenes scene)
    // { 
    //     // load switch scene Additive (with UI over all)
    //     _disposables.Add(SceneManager.LoadSceneAsync(_ctx.ScenesHandler.SwitchScene) // async load scene
    //         .AsAsyncOperationObservable() // as Observable thread
    //         .Do(x =>
    //         {
    //             // call during the process
    //             Debug.LogWarning($"[{this}][OnSwitchScene] Async load scene {_ctx.ScenesHandler.SwitchScene} progress: " +
    //                       x.progress); // show progress
    //         }).Subscribe(async _ =>
    //         {
    //             Debug.LogWarning($"[{this}][OnSwitchScene] Async load scene {_ctx.ScenesHandler.SwitchScene} done");
    //             _currentScene?.Exit();
    //             _currentScene?.Dispose();
    //             await OnSwitchSceneLoaded(scene);
    //         }));
    // }

    // private async Task OnSwitchSceneLoaded(GameScenes scene)
    // {
    //     _ctx.CursorSettings.EnableCursor(false);
    //
    //     var onLoadingProcess = new ReactiveProperty<string>().AddTo(_disposables);
    //     var switchSceneEntity = _ctx.ScenesHandler.LoadingSceneEntity(onLoadingProcess, scene);
    //
    //     var toLevelScene = scene == GameScenes.Level;
    //
    //     if (toLevelScene)
    //     {
    //         _ctx.Blocker.EnableScreenFade(true);
    //         _ctx.VideoManager.EnableVideo(true);
    //     }
    //
    //     Debug.Log($"[{this}][OnSwitchSceneLoaded] Start load scene {scene}");
    //
    //     _currentScene = await _ctx.ScenesHandler.SceneEntity(scene);
    //     if (_tokenSource.IsCancellationRequested) return;
    //
    //     SceneManager.LoadSceneAsync(_ctx.ScenesHandler.GetSceneName(scene)) // async load scene
    //         .AsAsyncOperationObservable() // as Observable thread
    //         .Do(x =>
    //         {
    //             // call during the process
    //             Debug.Log($"[{this}][OnSwitchSceneLoaded] Async load scene {scene} progress: " +
    //                       x.progress); // show progress
    //             onLoadingProcess.Value = x.progress.ToString();
    //         }).Subscribe(_ =>
    //         {
    //             Debug.Log($"[{this}][OnSwitchSceneLoaded] Async load scene {scene} done");
    //             switchSceneEntity.Exit();
    //             switchSceneEntity.Dispose();
    //
    //             _currentScene.Enter();
    //         }).AddTo(_disposables);
    // }

    public void Dispose()
    {
        _tokenSource.Cancel();
        _currentScene?.Dispose();
        _disposables.Dispose();
    }
}