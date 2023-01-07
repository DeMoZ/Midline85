using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MenuButtonView : AbstractMySelectable
    {
        [SerializeField] private Image defaultButton = default;
        [SerializeField] private Image hoverButton = default;
        
        [SerializeField] protected Color textDefaultColor = default;
        [SerializeField] protected Color textHoverColor = default;
        [SerializeField] private TextMeshProUGUI text = default;
        [SerializeField] private ButtonAudioSettings buttonAudioSettings = default;
        [SerializeField] private CursorSet cursorSettings = default;
        
        private static event Action<MenuButtonView> onHoverTransition;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            onHoverTransition += OnHoverTransition;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            onHoverTransition -= OnHoverTransition;
        }
        
        public void PlayHoverSound()
        {
            buttonAudioSettings?.PlayHoverSound();
        }

        private void OnHoverTransition(MenuButtonView btn)
        {
            SetButtonState(btn == this);
        }

        protected virtual void SetButtonState(bool toHover)
        {
            defaultButton?.gameObject.SetActive(!toHover);
            hoverButton?.gameObject.SetActive(toHover);
            text.color = toHover ? textHoverColor : textDefaultColor;
        }
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            switch (state)
            {
                case SelectionState.Normal:
                    cursorSettings?.ApplyCursor(CursorType.Normal);
                    SetButtonState(false);
                    break;
                case SelectionState.Highlighted:
                    if(gameObject.NotSelected())
                        gameObject.Select();
                    break;
                case SelectionState.Pressed:
                    SetButtonState(true);
                    buttonAudioSettings?.PlayClickSound();
                    break;
                case SelectionState.Selected:
                    onHoverTransition?.Invoke(this);
                    currentSelection = this;
                    SetButtonState(true);
                    PlayHoverSound();
                    break;
                case SelectionState.Disabled:
                    SetButtonState(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}