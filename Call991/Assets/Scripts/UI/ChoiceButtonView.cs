using System;
using I2.Loc;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChoiceButtonView : AaSelectable, IDisposable
    {
        [Space] [SerializeField] private Image defaultButton = default;
        [SerializeField] private Image hoverButton = default;
        [SerializeField] private ButtonLock buttonLock = default;
        [SerializeField] private TextMeshProUGUI text = default;
        [SerializeField] private Animation unlockAnimation = default;

        private LocalizedString _localize;
        private CompositeDisposable _disposables;

        public struct Ctx
        {
            public int Index;
            public ReactiveCommand<int> OnClickChoiceButton;
            public ReactiveCommand<int> OnAutoSelectButton;
        }

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();
            _ctx.OnClickChoiceButton.Subscribe(_ => OnClickChoiceButton()).AddTo(_disposables);
            _ctx.OnAutoSelectButton.Subscribe(OnAutoSelectButton).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void OnClickChoiceButton()
        {
            interactable = false;
        }

        private void OnAutoSelectButton(int index)
        {
            if (_ctx.Index != index) return;

            SetPressed();
            SetButtonState(true);
        }

        public void Show(string localizationKey, bool isLocked, bool showUnlock)
        {
            interactable = !isLocked;
            text.gameObject.SetActive(!isLocked);
            buttonLock.gameObject.SetActive(isLocked);
            unlockAnimation.Stop();

            if (showUnlock)
            {
                buttonLock.gameObject.SetActive(true);
                unlockAnimation.Play();
            }
            
            SetButtonState(false);

            _localize = localizationKey;
            text.text = _localize;
            gameObject.SetActive(true);
        }
        
        protected override void SetPressed()
        {
            base.SetPressed();
            _ctx.OnClickChoiceButton.Execute(_ctx.Index);
        }

        public override void SetDisabled()
        {
            base.SetDisabled();
            SetButtonState(false);
        }

        public override void SetNormal()
        {
            base.SetNormal();
            SetButtonState(false);
        }

        protected override void SetSelected()
        {
            base.SetSelected();
            SetButtonState(true);
        }

        private void SetButtonState(bool toHover)
        {
            defaultButton.gameObject.SetActive(!toHover);
            hoverButton.gameObject.SetActive(toHover);
        }
    }
}