using System;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AaSelectable : Selectable
{
    private bool _instant;

    public event Action OnClick;
    public event Action<AaSelectable> OnSelect;
    public event Action<AaSelectable> OnUnSelect;

    public bool IsSelected => currentSelectionState == SelectionState.Selected;

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
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
                Debug.LogError($"To Selected {currentSelectionState} -> Selected" + gameObject.ToStringEventSystem());
                DoStateTransition(SelectionState.Selected, instant);
                break;
            case SelectionState.Pressed:
                OnClick?.Invoke();
                Debug.LogWarning($"2 {currentSelectionState} -> Pressed" + gameObject.ToStringEventSystem());
                break;
            case SelectionState.Selected:
                Debug.Log($"2 {currentSelectionState} -> Selected" + gameObject.ToStringEventSystem());
                base.DoStateTransition(state, instant);
                SetSelected();
                OnSelect?.Invoke(this);
                break;
            case SelectionState.Disabled:
                Debug.Log($"2 {currentSelectionState} -> {state}" + gameObject.ToStringEventSystem());
                base.DoStateTransition(state, instant);
                SetDisabled();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void SetDisabled()
    {
        //throw new NotImplementedException();
    }

    private void SetSelected()
    {
        //throw new NotImplementedException();
    }

    private void SetNormal()
    {
        //throw new NotImplementedException();
    }

    public void Press()
    {
        DoStateTransition(SelectionState.Pressed, true);
    }
}