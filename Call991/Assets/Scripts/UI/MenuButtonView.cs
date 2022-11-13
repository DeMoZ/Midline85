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
        private bool _isSelected;

        public event Action OnClick;
        public event Action OnHover;

        public void InvokeOnClick()
        {
            OnClick?.Invoke();
            buttonAudioSettings.PlayClickSound();
        }

        public void InvokeOnHover()
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
                    SetHoverColor(false);
                    break;
                case SelectionState.Highlighted:
                    SetHoverColor(true);
                    InvokeOnHover();
                    break;
                case SelectionState.Pressed:
                    SetHoverColor(true);
                    InvokeOnClick();
                    break;
                case SelectionState.Selected:
                    _isSelected = true;
                    SetHoverColor(true);
                    InvokeOnHover();
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