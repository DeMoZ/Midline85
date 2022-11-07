using System;
using System.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Blocker : IDisposable
{
    private const float FadeTime = 0.5f;
    
    private readonly Image _screenBLocker;
    private readonly Image _videoBLocker;
    private readonly CompositeDisposable _disposables;
    
    // /// <summary>
    // /// true to 1, false to 0
    // /// </summary>
    // public ReactiveCommand<bool> OnFadeVideoBlocker = new ReactiveCommand<bool>();
    //     
    // /// <summary>
    // /// true to 1, false to 0
    // /// </summary>
    // public ReactiveCommand<bool> OnFadeScreenBlocker = new ReactiveCommand<bool>();
    //     
    // /// <summary>
    // /// true = enabled, false = disabled
    // /// </summary>
    // public ReactiveCommand<bool> OnEnableVideoBLocker = new ReactiveCommand<bool>();
    //     
    // /// <summary>
    // /// true = enabled, false = disabled
    // /// </summary>
    // public ReactiveCommand<bool> OnEnableScreenBLocker = new ReactiveCommand<bool>();
    //     
    // /// <summary>
    // /// true = 1, false = 0
    // /// </summary>
    // public ReactiveCommand<bool> OnSetVideoBlockerAlpha = new ReactiveCommand<bool>();
    //     
    // /// <summary>
    // /// true = 1, false = 0
    // /// </summary>
    // public ReactiveCommand<bool> OnSetScreenBLockerAlpha = new ReactiveCommand<bool>();
    
    private Color _videoBlockerColor;
    private Color _screenBlockerColor;
    public Blocker(Image screenBLocker, Image videoBLocker)
    {
        _screenBLocker = screenBLocker;
        _videoBLocker = videoBLocker;

        _videoBlockerColor = _videoBlockerColor;
        _screenBlockerColor = _screenBLocker.color;
        
        _disposables = new CompositeDisposable();
        
        // // required to return value from task (Func)
        // OnFadeVideoBlocker.Subscribe().AddTo(_disposables);
        // OnFadeScreenBlocker.Subscribe().AddTo(_disposables);
        // OnEnableVideoBLocker.Subscribe().AddTo(_disposables);
        // OnEnableScreenBLocker.Subscribe().AddTo(_disposables);
        // OnSetVideoBlockerAlpha.Subscribe().AddTo(_disposables);
        // OnSetScreenBLockerAlpha.Subscribe().AddTo(_disposables);
    }
    
    public async Task FadeVideoBlocker(bool show)
    {
        var toColor = show ? 1 : 0;
        _videoBlockerColor.a = show ? 0 : 1;
        _videoBLocker.color = _videoBlockerColor;

        var seq = _videoBLocker.DOFade(toColor, FadeTime);
        seq.Play();

        while (seq.IsPlaying())
            await Task.Yield();
    }
    
    public async Task FadeScreenBlocker(bool show, float? fadeTime = null)
    {
        var time = fadeTime ?? FadeTime;
        var toColor = show ? 1 : 0;
        _screenBlockerColor.a = show ? 0 : 1;
        _screenBLocker.color = _screenBlockerColor;
        
        var seq = _screenBLocker.DOFade(toColor, time);
        seq.Play();

        while (seq.IsPlaying())
            await Task.Yield();
    }

    public void EnableVideoBlocker(bool enable, bool show)
    {
        _videoBlockerColor.a = show ? 1 : 0;
        _videoBLocker.color = _videoBlockerColor;
        _videoBLocker.gameObject.SetActive(enable);
    }
    
    public void EnableScreenBlocker(bool enable, bool show)
    {
        _screenBlockerColor.a = show ? 1 : 0;
        _screenBLocker.color = _screenBlockerColor;
        _screenBLocker.gameObject.SetActive(enable);
    }
    
    public void Dispose()
    {
        _disposables.Dispose();
    }
}