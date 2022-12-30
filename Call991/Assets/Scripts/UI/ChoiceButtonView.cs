using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChoiceButtonView : AbstractChoiceButtonView
    {
        [SerializeField] private Image defaultButton = default;
        [SerializeField] private Image hoverButton = default;
        [SerializeField] private Image lockImage = default;

        [Space] [SerializeField] private Color defaultTextColor = default;
        [SerializeField] private Color hoverTextColor = default;
        [SerializeField] private TextMeshProUGUI text = default;

        [Space] [SerializeField] private ButtonAudioSettings buttonAudioSettings = default;
        [SerializeField] private CursorSet cursorSettings = default;

        private LocalizedString _localize;

        private static event Action<ChoiceButtonView> onHoverTransition;
        private static event Action<ChoiceButtonView> onPressTransition;

        private void OnEnable()
        {
            onHoverTransition += OnHoverTransition;
            onPressTransition += OnPressTransition;
        }

        private void OnDisable()
        {
            onHoverTransition -= OnHoverTransition;
            onPressTransition -= OnPressTransition;
        }
        
        public override void Show(string localizationKey, bool isLocked)
        {
            interactable = !isLocked;
            lockImage.gameObject.SetActive(isLocked);
            SetButtonState(false);

            _localize = localizationKey;
            text.text = _localize;

            defaultButton.DOFade(1, _ctx.buttonsAppearDuration);
            hoverButton.DOFade(1, _ctx.buttonsAppearDuration);
            text.DOFade(1, _ctx.buttonsAppearDuration);
            lockImage.DOFade(1, _ctx.buttonsAppearDuration);

            gameObject.SetActive(true);
        }

        public override void Choose()
        {
            onPressTransition.Invoke(this);
        }

        private void SetButtonState(bool toHover)
        {
            defaultButton.gameObject.SetActive(!toHover);
            hoverButton.gameObject.SetActive(toHover);
            text.color = toHover ? hoverTextColor : defaultTextColor;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            switch (state)
            {
                case SelectionState.Normal:
                    SetButtonState(false);
                    break;
                case SelectionState.Highlighted:
                    onHoverTransition?.Invoke(this);
                    break;
                case SelectionState.Pressed:
                    onPressTransition?.Invoke(this);
                    _ctx.onClickChoiceButton?.Execute(_ctx.index);
                    break;
                case SelectionState.Selected:
                    onHoverTransition?.Invoke(this);
                    break;
                case SelectionState.Disabled:

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private IEnumerator Hide(bool slow)
        {
            var duration = slow ? _ctx.slowButtonFadeDuration : _ctx.fastButtonFadeDuration;

            defaultButton.DOFade(0, duration);
            hoverButton.DOFade(0, duration);
            lockImage.DOFade(0, duration);

            if(!slow)
                text.DOFade(0, duration);
            
            yield return new WaitForSeconds(duration/2);

            if(slow)
                text.DOFade(0, duration);
            
            yield return new WaitForSeconds(duration/2);

            if (slow)
                cursorSettings.ApplyCursor(CursorType.Normal);
        }
        
        private void OnHoverTransition(ChoiceButtonView btn)
        {
            if (btn == this)
            {
                SetButtonState(true);
                PlayHoverSound();
            }
            else
            {
                SetButtonState(false);
            }
        }

        private void OnPressTransition(ChoiceButtonView btn)
        {
            interactable = false;
            StartCoroutine(Hide(btn == this));
        }

        private void PlayHoverSound()
        {
            buttonAudioSettings.PlayHoverSound();
        }
    }
}