using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MenuButtonView : Selectable
    {
        [SerializeField] private Color textNormal = default;
        [SerializeField] private Color textHover = default;
        [SerializeField] private TextMeshProUGUI text = default;
        [SerializeField] private ButtonAudioSettings buttonAudioSettings = default;
        [SerializeField] private CursorSet cursorSettings = default;
        
        private bool _isSelected;
        private bool _isHighlighted;

        public event Action OnClick;
        public event Action OnHover;

        public void PlayClickSound()
        {
            OnClick?.Invoke();
            buttonAudioSettings.PlayClickSound();
        }

        public void PlayHoverSound()
        {
            OnHover?.Invoke();
            buttonAudioSettings.PlayHoverSound();
        }

        private void SetHoverColor(bool hover)
        {
            text.color = hover
                ? textHover
                : textNormal;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            _isSelected = false;

            switch (state)
            {
                case SelectionState.Normal:
                    cursorSettings.ApplyCursor(CursorType.Normal);
                    SetHoverColor(false);
                    _isHighlighted = false;
                    break;
                case SelectionState.Highlighted:
                    cursorSettings.ApplyCursor(CursorType.CanClick);
                    _isHighlighted = true;
                    SetHoverColor(true);
                    PlayHoverSound();
                    break;
                case SelectionState.Pressed:
                    SetHoverColor(true);
                    PlayClickSound();
                    break;
                case SelectionState.Selected:
                    _isSelected = true;
                    SetHoverColor(true);
                    
                    if (!_isHighlighted)
                    {
                        PlayHoverSound();
                    }
                    
                    break;
                case SelectionState.Disabled:
                    SetHoverColor(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void Update()
        {
            if (_isSelected && Input.GetKey(KeyCode.Return))
                OnClick?.Invoke();
        }
    }
}