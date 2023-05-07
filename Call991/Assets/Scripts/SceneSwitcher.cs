using System;
using System.Threading.Tasks;
using Configs;
using Data;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<GameScenes> onSwitchScene;
        public ScenesHandler scenesHandler;
        public VideoManager videoManager;
        public Blocker blocker;
        public GameSet gameSet;
        public CursorSet cursorSettings;
    }

    private Ctx _ctx;
    private CompositeDisposable _diposables;

    private IGameScene _currentScene;

    public SceneSwitcher(Ctx ctx)
    {
        _ctx = ctx;
        _diposables = new CompositeDisposable();
        _ctx.onSwitchScene.Subscribe(OnSwitchScene).AddTo(_diposables);
    }

    private void OnSwitchScene(GameScenes scene)
    {
        // if (toLevelScene)
        // {
        //     // Enable blocker
        //     
        //     // load video preload
        //     // Fade disable blocker after video loaded
        //     
        //     _ctx.videoManager.EnableVideoBlocker(true);
        // }
        // else
        // {
        //     // disable video
        //     // disable blocker
        // }

        // load switch scene Additive (with UI over all)
        _diposables.Add(SceneManager.LoadSceneAsync(_ctx.scenesHandler.SwitchScene) // async load scene
            .AsAsyncOperationObservable() // as Observable thread
            .Do(x =>
            {
                // call during the process
                Debug.Log($"[{this}][OnSwitchScene] Async load scene {_ctx.scenesHandler.SwitchScene} progress: " +
                          x.progress); // show progress
            }).Subscribe(async _ =>
            {
                Debug.Log($"[{this}][OnSwitchScene] Async load scene {_ctx.scenesHandler.SwitchScene} done");
                _currentScene?.Exit();
                _currentScene?.Dispose();
                await OnSwitchSceneLoaded(scene);
            }));
    }

    private async Task OnSwitchSceneLoaded(GameScenes scene)
    {
        _ctx.cursorSettings.EnableCursor(false);

        var onLoadingProcess = new ReactiveProperty<string>().AddTo(_diposables);
        var switchSceneEntity = _ctx.scenesHandler.LoadingSceneEntity(onLoadingProcess, scene);

        var toLevelScene = scene == GameScenes.Level1;

        if (toLevelScene)
        {
            _ctx.blocker.EnableScreenFade(true);
            await _ctx.videoManager.LoadVideoSoToPrepareVideo(_ctx.gameSet.titleVideoSoName);
            _ctx.videoManager.EnableVideo(true);
            _ctx.videoManager.PlayPreparedVideo();
            await _ctx.blocker.FadeScreenBlocker(false);
            await Task.Delay((int)(_ctx.gameSet.startGameOpeningHoldTime * 1000));
        }

        Debug.Log($"[{this}][OnSwitchSceneLoaded] Start load scene {scene}");

        _currentScene = await _ctx.scenesHandler.SceneEntity(scene);
        
        SceneManager.LoadSceneAsync(_ctx.scenesHandler.GetSceneName(scene)) // async load scene
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
        _diposables.Dispose();
        _currentScene.Dispose();
    }
}