using System;
using UniRx;
using UnityEngine;

public class PhraseSkipper : IDisposable
{
    private CompositeDisposable _disposables;

    public PhraseSkipper(ReactiveCommand onSkipPhrase)
    {
        _disposables = new CompositeDisposable();

#if UNITY_EDITOR
        var clickStream = Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(1));

        /*clickStream.Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(250)))
            .Where(xs => xs.Count >= 2)
            .Subscribe(xs => Debug.Log("DoubleClick Detected! Count:" + xs.Count));*/

        clickStream.Subscribe(xs =>
        {
            if (Time.timeScale == 0) return;

            onSkipPhrase.Execute();
        }).AddTo(_disposables);
#endif
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}