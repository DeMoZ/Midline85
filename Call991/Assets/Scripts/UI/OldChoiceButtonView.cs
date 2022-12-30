using System;
using System.Threading.Tasks;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace UI
{
    public class OldChoiceButtonView : AbstractChoiceButtonView
    {
       
        [Space] [SerializeField] private GameObject blocked = default;
        [Space] [SerializeField] private Color textNormal = default;
        [SerializeField] private Color textHover = default;
        [Space] [SerializeField] private TextMeshProUGUI text = default;
        [SerializeField] private TextMeshProUGUI textSelected = default;
        [SerializeField] private CanvasGroup canvasGroup = default;
        [SerializeField] private ButtonAudioSettings buttonAudioSettings = default;
        [SerializeField] private CursorSet cursorSettings = default;
        
        private LocalizedString _localize;

        private bool _isSelected;
        private bool _isHighlighted;
        
        public void PlayHoverSound()
        {
            buttonAudioSettings.PlayHoverSound();
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            _isSelected = false;

            // Debug.LogWarning($"choice button {name} {state}");

            switch (state)
            {
                case SelectionState.Normal:
                    cursorSettings?.ApplyCursor(CursorType.Normal);
                    SetHoverColor(false);
                    _isHighlighted = false;
                    break;
                case SelectionState.Highlighted:
                    cursorSettings?.ApplyCursor(CursorType.CanClick);
                    SetHoverColor(true);
                    PlayHoverSound();
                    _isHighlighted = true;
                    break;
                case SelectionState.Pressed:
                    SetHoverColor(true);
                    _ctx.onClickChoiceButton.Execute(_ctx.index);
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

        private void SetHoverColor(bool hover)
        {
            if(text)
                text.color = hover
                    ? textHover
                    : textNormal;
        }

        public void SetClicked()
        {
            text.gameObject.SetActive(false);
            textSelected.gameObject.SetActive(true);
        }

        public override async void Show(string choiceKey, bool isBlocked = false)
        {
            _isHighlighted = false;
            _localize = choiceKey;
            text.text = _localize;
            textSelected.text = _localize;
            text.gameObject.SetActive(true);
            textSelected.gameObject.SetActive(false);
            blocked.SetActive(isBlocked);
            gameObject.SetActive(true);
            canvasGroup.DOFade(1, _ctx.buttonsAppearDuration);
            await Task.Delay((int) (_ctx.buttonsAppearDuration * 1000));
        }

        public override void Choose()
        {
            throw new NotImplementedException();
        }

        public async void Hide(bool slow)
        {
            var duration = slow ? _ctx.slowButtonFadeDuration : _ctx.fastButtonFadeDuration;
            canvasGroup.DOFade(0, duration);
            await Task.Delay((int) (duration * 1000));
            
            if (_isHighlighted)
                cursorSettings.ApplyCursor(CursorType.Normal);
        }

        private void Update()
        {
            if (_isSelected && Input.GetKey(KeyCode.Return))
            {
                SetHoverColor(true);
                _ctx.onClickChoiceButton.Execute(_ctx.index);
            }
        }
    }
}