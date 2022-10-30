using System;
using System.Threading.Tasks;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ChoiceButtonView : Selectable, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public struct Ctx
        {
            public int index;
            public ReactiveCommand<int> onClickChoiceButton;
            public float buttonsAppearDuration;
            public float fastButtonFadeDuration;
            public float slowButtonFadeDuration;
        }

        [Space] [SerializeField] private GameObject blocked = default;
        [Space] [SerializeField] private Color textNormal = default;
        [SerializeField] private Color textHover = default;
        [SerializeField] private Color textClicked = default;
        [SerializeField] private Color textBlocked = default;
        [Space] [SerializeField] private TextMeshProUGUI text = default;
        [SerializeField] private TextMeshProUGUI textSelected = default;
        [SerializeField] private CanvasGroup canvasGroup = default;

        private Ctx _ctx;

        private static bool _isClicked;
        private LocalizedString _localize;

        private bool _isSelected;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            _isSelected = false;

            Debug.LogWarning($"choice button {name} {state}");

            switch (state)
            {
                case SelectionState.Normal:
                    SetHoverColor(false);
                    break;
                case SelectionState.Highlighted:
                    SetHoverColor(true);
                    break;
                case SelectionState.Pressed:
                    SetHoverColor(true);
                    _ctx.onClickChoiceButton.Execute(_ctx.index);
                    break;
                case SelectionState.Selected:
                    _isSelected = true;
                    SetHoverColor(true);
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
            text.color = hover
                ? textHover
                : textNormal;
        }

        public void SetClicked()
        {
            text.gameObject.SetActive(false);
            textSelected.gameObject.SetActive(true);
        }

        public async void Show(string choiceKey, bool isBlocked = false)
        {
            _isClicked = false;
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

        public async void Hide(bool slow)
        {
            var duration = slow ? _ctx.slowButtonFadeDuration : _ctx.fastButtonFadeDuration;
            canvasGroup.DOFade(0, duration);
            await Task.Delay((int) (duration * 1000));
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