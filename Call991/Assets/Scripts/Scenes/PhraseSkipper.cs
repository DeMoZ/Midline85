using System;
using UniRx;
using UnityEngine;

public class PhraseSkipper :IDisposable
{
    private CompositeDisposable _disposables;
    public PhraseSkipper(ReactiveCommand onSkipPhrase)
    {
        _disposables = new CompositeDisposable();
        
        var clickStream = Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(1));
        
        /*clickStream.Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(250)))
            .Where(xs => xs.Count >= 2)
            .Subscribe(xs => Debug.Log("DoubleClick Detected! Count:" + xs.Count));*/

        clickStream.Subscribe(xs =>
        {
            onSkipPhrase.Execute();
        }).AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}