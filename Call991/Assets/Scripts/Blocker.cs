using System;
using System.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Blocker : IDisposable
{
    private const float FadeTime = 0.3f;

    private readonly Image _screenFade;
    private readonly Image _videoFade;
    private readonly CompositeDisposable _disposables;
    
    private Color _videoBlockerColor;
    private Color _screenBlockerColor;

    public Blocker(Image screenFade, Image videoFade)
    {
        _screenFade = screenFade;
        _videoFade = videoFade;

        _videoBlockerColor = _videoFade.color;
        _screenBlockerColor = _screenFade.color;

        _disposables = new CompositeDisposable();
    }

    public async Task FadeVideoBlocker(bool show)
    {
        var toColor = show ? 1 : 0;
        _videoBlockerColor.a = show ? 0 : 1;
        _videoFade.color = _videoBlockerColor;
        
        EnableScreenFade(true);
        
        var seq = _videoFade.DOFade(toColor, FadeTime);
        seq.Play();

        while (seq.IsPlaying())
            await Task.Yield();
        
        if (!show)
            EnableScreenFade(false);
    }

    public async Task FadeScreenBlocker(bool show, float? fadeTime = null)
    {
        var time = fadeTime ?? FadeTime;
        var toColor = show ? 1 : 0;
        _screenBlockerColor.a = show ? 0 : 1;
        _screenFade.color = _screenBlockerColor;

        EnableScreenFade(true);

        var seq = _screenFade.DOFade(toColor, time);
        seq.Play();

        while (seq.IsPlaying())
            await Task.Yield();
        
        if (!show)
            EnableScreenFade(false);
    }

    public void EnableVideoFade(bool enable)
    {
        _videoBlockerColor.a = enable ? 1 : 0;
        _videoFade.color = _videoBlockerColor;
        _videoFade.gameObject.SetActive(enable);
    }

    public void EnableScreenFade(bool enable)
    {
        _screenBlockerColor.a = enable ? 1 : 0;
        _screenFade.color = _screenBlockerColor;
        _screenFade.gameObject.SetActive(enable);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}