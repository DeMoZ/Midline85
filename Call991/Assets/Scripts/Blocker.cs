using System;
using System.Threading.Tasks;
using Configs;
using Core;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Blocker : IDisposable
{
    public struct Ctx
    {
        public Image ScreenFade;
        public Image VideoFade;
        public GameSet GameSet;
        public ReactiveProperty<bool> IsPauseAllowed;
        public ReactiveCommand<(bool show, float time)> OnScreenFade;
    }
    
    private readonly Image _screenFade;
    //private readonly Image _videoFade;
    private readonly CompositeDisposable _disposables;
    private readonly GameSet _gameSet;

    //private Color _videoBlockerColor;
    private Color _screenBlockerColor;
    private Ctx _ctx;

    public Blocker(Ctx ctx)
    {
        _ctx = ctx;
        _disposables = new CompositeDisposable();

        _screenFade = _ctx.ScreenFade;
        //_videoFade = _ctx.VideoFade;

        //_videoBlockerColor = _videoFade.color;
        _screenBlockerColor = _screenFade.color;
        _gameSet = _ctx.GameSet;

        _ctx.OnScreenFade.Subscribe(param =>
        {
            Debug.Log($"[{this}] received event Fade");
            FadeScreenBlocker(param.show, param.time).Forget();
        }).AddTo(_disposables);
    }

    // public async Task FadeVideoBlocker(bool show, float? fadeTime = null)
    // {
    //     var time = fadeTime ?? _gameSet.shortFadeTime;
    //     var toColor = show ? 1 : 0;
    //     _videoBlockerColor.a = show ? 0 : 1;
    //     _videoFade.color = _videoBlockerColor;
    //
    //     EnableVideoFade(true, !show);
    //
    //     var seq = _videoFade.DOFade(toColor, time);
    //     seq.Play();
    //
    //     while (seq.IsPlaying())
    //         await Task.Yield();
    //
    //     if (!show)
    //         EnableVideoFade(false);
    // }

    public async Task FadeScreenBlocker(bool show, float? fadeTime = null)
    {
        _ctx.IsPauseAllowed.Value = false;
        var time = fadeTime ?? _gameSet.shortFadeTime;
        var toColor = show ? 1 : 0;
        _screenBlockerColor.a = show ? 0 : 1;
        _screenFade.color = _screenBlockerColor;

        EnableScreenFade(true, !show);

        var seq = _screenFade.DOFade(toColor, time);
        seq.SetUpdate(true);
        seq.Play();

        while (seq.IsPlaying())
            await Task.Yield();

        if (!show)
            EnableScreenFade(false);
        
        _ctx.IsPauseAllowed.Value = true;
    }

    // public void EnableVideoFade(bool enable, bool show = true)
    // {
    //     _videoBlockerColor.a = show ? 1 : 0;
    //     _videoFade.color = _videoBlockerColor;
    //     _videoFade.gameObject.SetActive(enable);
    // }

    public void EnableScreenFade(bool enable, bool show = true)
    {
        Debug.Log($"[{this}] Screen fade enable {enable}; show {show}");
        _screenBlockerColor.a = show ? 1 : 0;
        _screenFade.color = _screenBlockerColor;
        _screenFade.gameObject.SetActive(enable);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}