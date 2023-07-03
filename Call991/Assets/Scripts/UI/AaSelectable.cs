using System;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AaSelectable : Selectable
{
    private const float CoolTime = 0.3f;

    [SerializeField] private ButtonAudioSettings buttonAudioSettings = default;
    [SerializeField] private CursorSet cursorSettings = default;

    private bool _instant;
    private bool _noPointerPress;
    private float _coolTimer;

    public event Action OnClick;
    public event Action<AaSelectable> OnSelectObj;
    public event Action<AaSelectable> OnUnSelect;
    public static event Action<Vector2, Sprite> OnMouseClickSelectable;

    private bool IsNormal => currentSelectionState == SelectionState.Normal;

    private void Update()
    {
        if (_coolTimer > 0)
            _coolTimer -= Time.deltaTime;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        DoStateTransition(SelectionState.Normal, _instant);
    }

    protected void DoStateTransitionNormal()
    {
        DoStateTransition(SelectionState.Normal, _instant);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (state == SelectionState.Pressed)
        {
            Debug.LogWarning($"selectable to next state <color=yellow>Pressed {name}</color>");
        }
        
        _instant = instant;

        switch (state)
        {
            case SelectionState.Normal:
                SetNormal();

                if (!IsNormal) 
                    OnUnSelect?.Invoke(this);

                break;
            case SelectionState.Highlighted:
                SetSelected();
                PlayHoverSound();
                break;
            case SelectionState.Pressed:
                SetPressed();
                break;
            case SelectionState.Selected:
                SetSelected();
                PlayHoverSound();
                OnSelectObj?.Invoke(this);
                break;
            case SelectionState.Disabled:
                SetDisabled();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        base.DoStateTransition(state, instant);
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
        _noPointerPress = true;
        buttonAudioSettings?.PlayClickSound();
        DoStateTransition(SelectionState.Pressed, true);
    }

    /// <summary>
    /// Nomal button behaviour
    /// </summary>
    protected virtual void SetPressed()
    {
        if (_coolTimer > 0) return;

        Debug.LogWarning($"selectable to next state <color=red>Pressed {name}</color>");
        _coolTimer = CoolTime;
        OnClick?.Invoke();

        if (!_noPointerPress && cursorSettings.ClickPointSprite)
            OnMouseClickSelectable?.Invoke((Vector2)Input.mousePosition + cursorSettings.ClickPointOffset,
                cursorSettings.ClickPointSprite);

        _noPointerPress = false;
    }
}