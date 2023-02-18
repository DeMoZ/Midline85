using System;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AaSelectable : Selectable
{
    [SerializeField] private ButtonAudioSettings buttonAudioSettings = default;
    [SerializeField] private CursorSet cursorSettings = default;
    
    private bool _instant;

    public event Action OnClick;
    public event Action<AaSelectable> OnSelect;
    public event Action<AaSelectable> OnUnSelect;

    public bool IsSelected => currentSelectionState == SelectionState.Selected;
    public bool IsPressed => IsPressed();

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        DoStateTransition(SelectionState.Normal, _instant);
    }

    protected void DoStateTransitionNormal()
    {
        DoStateTransition(SelectionState.Normal, _instant);    
    }
    
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        _instant = instant;

        switch (state)
        {
            case SelectionState.Normal:
                if (currentSelectionState == SelectionState.Selected)
                {
                    OnUnSelect?.Invoke(this);
                }

                base.DoStateTransition(state, instant);
                SetNormal();
                break;
            case SelectionState.Highlighted:
                //Debug.LogWarning($"[AaSelectable] To Selected {currentSelectionState} -> Selected" + gameObject.ToStringEventSystem());
                DoStateTransition(SelectionState.Selected, instant);
                break;
            case SelectionState.Pressed:
                SetPressed();
                break;
            case SelectionState.Selected:
                //Debug.Log($"[AaSelectable] 2 {currentSelectionState} -> Selected" + gameObject.ToStringEventSystem());
                base.DoStateTransition(state, instant);
                SetSelected();
                PlayHoverSound();
                OnSelect?.Invoke(this);
                break;
            case SelectionState.Disabled:
                //Debug.Log($"[AaSelectable] 2 {currentSelectionState} -> {state}" + gameObject.ToStringEventSystem());
                base.DoStateTransition(state, instant);
                SetDisabled();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    protected virtual void SetDisabled()
    {
    }

    protected virtual void SetSelected()
    {
        cursorSettings?.ApplyCursor(CursorType.CanClick);
    }

    protected virtual void SetNormal()
    {
        cursorSettings?.ApplyCursor(CursorType.Normal);
    }

    protected virtual void PlayHoverSound()
    {
        buttonAudioSettings?.PlayHoverSound();
    }

    /// <summary>
    /// For keyboard and timeout press -> SetPressed
    /// </summary>
    public void Press()
    {
        DoStateTransition(SelectionState.Pressed, true);
    }

    /// <summary>
    /// Nomal button behaviour
    /// </summary>
    protected virtual void SetPressed()
    {
        OnClick?.Invoke();
        //Debug.LogWarning($"[AaSelectable] 2 {currentSelectionState} -> Pressed" + gameObject.ToStringEventSystem());
    }
}