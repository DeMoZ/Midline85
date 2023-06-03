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
    private readonly Image _screenFade;
    private readonly Image _videoFade;
    private readonly CompositeDisposable _disposables;
    private readonly GameSet _gameSet;

    private Color _videoBlockerColor;
    private Color _screenBlockerColor;

    public Blocker(Image screenFade, Image videoFade, GameSet gameSet, ReactiveCommand<(bool show, float time)> onScreenFade)
    {
        _disposables = new CompositeDisposable();

        _screenFade = screenFade;
        _videoFade = videoFade;

        _videoBlockerColor = _videoFade.color;
        _screenBlockerColor = _screenFade.color;
        _gameSet = gameSet;

        onScreenFade.Subscribe(param =>
        {
            Debug.Log($"[{this}] received event Fade");
            FadeScreenBlocker(param.show, param.time).Forget();
        }).AddTo(_disposables);
    }

    public async Task FadeVideoBlocker(bool show, float? fadeTime = null)
    {
        var time = fadeTime ?? _gameSet.shortFadeTime;
        var toColor = show ? 1 : 0;
        _videoBlockerColor.a = show ? 0 : 1;
        _videoFade.color = _videoBlockerColor;

        EnableVideoFade(true, !show);

        var seq = _videoFade.DOFade(toColor, time);
        seq.Play();

        while (seq.IsPlaying())
            await Task.Yield();

        if (!show)
            EnableVideoFade(false);
    }

    public async Task FadeScreenBlocker(bool show, float? fadeTime = null)
    {
        var time = fadeTime ?? _gameSet.shortFadeTime;
        var toColor = show ? 1 : 0;
        _screenBlockerColor.a = show ? 0 : 1;
        _screenFade.color = _screenBlockerColor;

        EnableScreenFade(true, !show);

        var seq = _screenFade.DOFade(toColor, time);
        seq.Play();

        while (seq.IsPlaying())
            await Task.Yield();

        if (!show)
            EnableScreenFade(false);
    }

    public void EnableVideoFade(bool enable, bool show = true)
    {
        _videoBlockerColor.a = show ? 1 : 0;
        _videoFade.color = _videoBlockerColor;
        _videoFade.gameObject.SetActive(enable);
    }

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