using System;
using System.Threading.Tasks;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ChoiceButtonView : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public struct Ctx
        {
            public int index;
            public ReactiveCommand<int> onClickChoiceButton;
            public float buttonsAppearDuration;
            public float fastButtonFadeDuration;
            public float slowButtonFadeDuration;
        }

        [SerializeField] private GameObject normal = default;
        [SerializeField] private GameObject hover = default;
        [SerializeField] private GameObject clicked = default;
        [SerializeField] private GameObject blocked = default;
        [Space]
        [SerializeField] private Color textNormal = default;
        [SerializeField] private Color textHover = default;
        [SerializeField] private Color textClicked = default;
        [SerializeField] private Color textBlocked = default;
        [Space] 
        [SerializeField] private TextMeshProUGUI text = default;
        [SerializeField] private TextMeshProUGUI textSelected = default;
        [SerializeField] private CanvasGroup canvasGroup = default;

        private Ctx _ctx;
        private ButtonStates _currentState;

        private static bool _isClicked;
        private LocalizedString _localize;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        private void SwitchButtonState(ButtonStates state, bool checkClicked = false)
        {
            if (checkClicked)
                if (_currentState == ButtonStates.Clicked && state == ButtonStates.Normal)
                    state = ButtonStates.Clicked;

            if (state != ButtonStates.Hover)
                _currentState = state;

            normal?.SetActive(state is ButtonStates.Normal or ButtonStates.Blocked);
            hover?.SetActive(state == ButtonStates.Hover);
            clicked?.SetActive(state == ButtonStates.Clicked);
            blocked?.SetActive(state == ButtonStates.Blocked);

            switch (state)
            {
                case ButtonStates.Normal:
                    text.color = textNormal;
                    break;
                case ButtonStates.Hover:
                    text.color = textHover;
                    break;
                case ButtonStates.Clicked:
                    text.color = textClicked;
                    break;
                case ButtonStates.Blocked:
                    text.color = textBlocked;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void SetClicked()
        {
            text.gameObject.SetActive(false);
            textSelected.gameObject.SetActive(true);
            SwitchButtonState(ButtonStates.Clicked);
        }

        public async void Show(string choiceKey, bool isBlocked = false)
        {
            _isClicked = false;
            _localize = choiceKey;
            text.text = _localize;
            textSelected.text = _localize;
            text.gameObject.SetActive(true);
            textSelected.gameObject.SetActive(false);
            SwitchButtonState(ButtonStates.Normal);

            if (isBlocked)
                SwitchButtonState(ButtonStates.Blocked);

            gameObject.SetActive(true);
            canvasGroup.DOFade(1, _ctx.buttonsAppearDuration);
            await Task.Delay((int) (_ctx.buttonsAppearDuration * 1000));
        }

        public async void Hide(bool slow)
        {
            var duration = slow ? _ctx.slowButtonFadeDuration : _ctx.fastButtonFadeDuration;
            canvasGroup.DOFade(0, duration);
            await Task.Delay((int) (duration * 1000));
        }

        private enum ButtonStates
        {
            Normal,
            Hover,
            Clicked,
            Blocked,
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(_isClicked) return;
            if (_currentState == ButtonStates.Blocked) return;
            
            SwitchButtonState(ButtonStates.Hover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isClicked) return;
            if (_currentState == ButtonStates.Blocked) return;
            
            SwitchButtonState(ButtonStates.Normal, true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isClicked) return;
            if (_currentState == ButtonStates.Blocked) return;
            
            _isClicked = true;
            _ctx.onClickChoiceButton.Execute(_ctx.index);
        }
    }
}