using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChoiceButtonView : AaSelectable
    {
        [SerializeField] private Image defaultButton = default;
        [SerializeField] private Image hoverButton = default;
        [SerializeField] private Image lockImage = default;

        [Space] [SerializeField] private Color defaultTextColor = default;
        [SerializeField] private Color hoverTextColor = default;
        [SerializeField] private TextMeshProUGUI text = default;

        private LocalizedString _localize;

        private Tween _scalingTween;
        private float _scaledSize = 1.2f;
        private float _scaleDuration = 0.35f;

        private static event Action<ChoiceButtonView> OnChoiceDone;

        public struct Ctx
        {
            public int index;
            public ReactiveCommand<int> onClickChoiceButton;
            public float buttonsAppearDuration;
            public float fastButtonFadeDuration;
            public float slowButtonFadeDuration;
        }

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Show(string localizationKey, bool isLocked)
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

        protected override void OnEnable()
        {
            base.OnEnable();
            OnChoiceDone += ChoiceDoneForAllButtons;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnChoiceDone -= ChoiceDoneForAllButtons;
            _scalingTween?.Kill();
        }

        protected override void SetPressed()
        {
            base.SetPressed();
            OnChoiceDone?.Invoke(this);
        }

        protected override void SetDisabled()
        {
            base.SetDisabled();
            SetButtonState(false);
        }

        protected override void SetNormal()
        {
            base.SetNormal();
            SetButtonState(false);
        }

        protected override void SetSelected()
        {
            base.SetSelected();
            SetButtonState(true);
            _scalingTween = hoverButton.rectTransform.DOScale(_scaledSize, _scaleDuration).SetEase(Ease.OutBounce);
        }

        private void SetButtonState(bool toHover)
        {
            if (defaultButton)
                defaultButton.gameObject.SetActive(!toHover);

            if (hoverButton)
                hoverButton.gameObject.SetActive(toHover);

            text.color = toHover ? hoverTextColor : defaultTextColor;
            _scalingTween?.Kill();

            if (hoverButton && !toHover)
                hoverButton.rectTransform.localScale = Vector3.one;
        }

        private void ChoiceDoneForAllButtons(ChoiceButtonView btn)
        {
            if (btn == this)
                _ctx.onClickChoiceButton?.Execute(_ctx.index);
            else
                DoStateTransitionNormal();

            interactable = false;
            StartCoroutine(Hide(btn == this));
        }

        private IEnumerator Hide(bool slow)
        {
            var duration = slow ? _ctx.slowButtonFadeDuration : _ctx.fastButtonFadeDuration;

            defaultButton.DOFade(0, duration);
            hoverButton.DOFade(0, duration);
            lockImage.DOFade(0, duration);

            if (!slow)
                text.DOFade(0, duration);

            yield return new WaitForSeconds(duration / 2);

            if (slow)
                text.DOFade(0, duration);

            yield return new WaitForSeconds(duration / 2);

            if (slow)
                SetNormal();
        }
    }
}