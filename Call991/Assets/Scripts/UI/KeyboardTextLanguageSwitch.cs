using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class KeyboardTextLanguageSwitch : Selectable
{
    public UnityEvent _left;
    public UnityEvent _right;

    private bool _isSelected;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        _isSelected = false;

        switch (state)
        {
            case SelectionState.Normal:
                _isSelected = false;
                break;
            case SelectionState.Selected:
                _isSelected = true;
                break;
            case SelectionState.Disabled:
                _isSelected = false;
                break;
        }
    }

    void Update()
    {
        if (!_isSelected) return;

        if (Input.GetKey(KeyCode.LeftArrow))
            _left?.Invoke();

        if (Input.GetKey(KeyCode.RightArrow))
            _right?.Invoke();
    }
}