using System;
using UniRx;

/// <summary>
/// Composite Root Container for dialogue Events executed from objects
/// </summary>
public class ObjectEvents : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<(bool show, float time)> OnScreenFade;
        public ReactiveCommand<(bool show, string[] keys)> OnShowTitle;
        public ReactiveCommand<(bool show, string[] keys, float delayTime, float fadeTime)> OnShowWarning;
        public bool SkipTitle;
        public bool SkipWarning;
    }

    public Ctx EventsGroup { get; }

    public ObjectEvents(Ctx ctx)
    {
        EventsGroup = ctx;
    }
    
    public void Dispose()
    {
    }
}