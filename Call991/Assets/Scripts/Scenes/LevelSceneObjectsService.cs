using System;
using System.Collections.Generic;
using AaDialogueGraph;
using UniRx;

public class LevelSceneObjectsService : IDisposable
{
    /// <summary>
    /// send on this button clicked by player
    /// </summary>
    public ReactiveCommand<int> OnClickChoiceButton = new ();
    /// <summary>
    /// button is autoselected and it needs to send OnClickChoiceButton event
    /// </summary>
    public ReactiveCommand<int> OnAutoSelectButton = new ();
    public ReactiveCommand<List<ChoiceNodeData>> OnShowButtons = new ();
    public ReactiveCommand OnHideButtons  = new ();
    public ReactiveCommand OnBlockButtons = new ();

    public void Dispose()
    {
        OnClickChoiceButton?.Dispose();
        OnAutoSelectButton?.Dispose();
        OnShowButtons?.Dispose();
        OnHideButtons?.Dispose();
    }
}