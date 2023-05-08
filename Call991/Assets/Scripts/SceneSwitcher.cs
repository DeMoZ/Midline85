using System;
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
    }

    private Ctx _ctx;
    private CompositeDisposable _diposables;

    private IGameScene _currentScene;

    public SceneSwitcher(Ctx ctx)
    {
        _ctx = ctx;
        _diposables = new CompositeDisposable();
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

        var toLevelScene = scene == GameScenes.Level1;

        if (toLevelScene)
        {
            _ctx.Blocker.EnableScreenFade(true);
            var contentLoader = new ContentLoader(new ContentLoader.Ctx());
            var videoClip = await contentLoader.GetObjectAsync<VideoClip>(_ctx.GameSet.interactiveVideoRef);
            _ctx.VideoManager.EnableVideo(true);
            _ctx.VideoManager.PlayVideo(videoClip);
            await _ctx.Blocker.FadeScreenBlocker(false);
            await Task.Delay((int)(_ctx.GameSet.startGameOpeningHoldTime * 1000));
        }

        Debug.Log($"[{this}][OnSwitchSceneLoaded] Start load scene {scene}");

        _currentScene = await _ctx.ScenesHandler.SceneEntity(scene);
        
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
        _diposables.Dispose();
        _currentScene.Dispose();
    }
}