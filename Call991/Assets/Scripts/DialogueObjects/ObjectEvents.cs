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
    }

    public Ctx GetCtx { get; }

    public ObjectEvents(Ctx getCtx)
    {
        GetCtx = getCtx;
    }
    
    public void Dispose()
    {
    }
}