using System;
using UniRx;

/// <summary>
/// Composite Root Container for dialogue Events
/// </summary>
public class ObjectEvents : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<(bool show, float time)> OnScreenFade;
        public ReactiveCommand<(bool show, string[] keys)> OnShowTitle;
        public bool SkipTitle;
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