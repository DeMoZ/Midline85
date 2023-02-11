using System;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class AaSelectable : Selectable
{
    public event Action OnClick;
    public event Action<AaSelectable> OnHighlight;
    public event Action<AaSelectable> OnSelect;
    public event Action<AaSelectable> OnUnSelect;

    private SelectionState _previousState;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (state == SelectionState.Highlighted)
        {
            Debug.Log($"2 {_previousState} -> Selected" + gameObject.ToStringEventSystem());
            base.DoStateTransition(SelectionState.Selected, instant);
            state = SelectionState.Selected;
        }

        switch (state)
        {
            case SelectionState.Normal:
                base.DoStateTransition(state, instant);
                SetNormal();
                break;
            case SelectionState.Highlighted:
                // do nothing
                break;
            case SelectionState.Pressed:
                OnClick?.Invoke();
                Debug.LogWarning($"2 {_previousState} -> Pressed" + gameObject.ToStringEventSystem());
                break;
            case SelectionState.Selected:
                // //if (_previousState == SelectionState.Pressed) break;
                // if (_previousState == SelectionState.Selected)
                // {
                //     base.DoStateTransition(SelectionState.Normal, instant);
                // }
                // else
                //{
                    Debug.Log($"2 {_previousState} -> Selected" + gameObject.ToStringEventSystem());
                    base.DoStateTransition(state, instant);
                    SetSelected();
                    OnSelect?.Invoke(this);
                //}

                break;
            case SelectionState.Disabled:
                Debug.Log($"2 {_previousState} -> {state}" + gameObject.ToStringEventSystem());
                base.DoStateTransition(state, instant);
                SetDisabled();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        _previousState = state;
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