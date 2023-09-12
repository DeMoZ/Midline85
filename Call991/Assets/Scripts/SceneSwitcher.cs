using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public ScenesHandler ScenesHandler;
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
        _ctx.CursorSettings.ApplyCursor(CursorType.Normal);
        _currentScene.Enter();
    }
    
    public void Dispose()
    {
        _tokenSource.Cancel();
        _currentScene?.Dispose();
        _disposables.Dispose();
    }
}