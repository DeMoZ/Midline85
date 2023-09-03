using I2.Loc;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AaChoiceButton : AaButton
    {
        public struct Ctx
        {
            public int Index;
            public ReactiveCommand<int> OnClickChoiceButton;
            public ReactiveCommand<int> OnAutoSelectButton;
        }

        [Space] [SerializeField] private Image defaultButton = default;
        [SerializeField] private Image hoverButton = default;
        [SerializeField] private ButtonLock buttonLock = default;
        [SerializeField] private TextMeshProUGUI text = default;
        [SerializeField] private Animation unlockAnimation = default;

        private LocalizedString _localize;
        private CompositeDisposable _disposables;
        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();
            _ctx.OnClickChoiceButton.Subscribe(_ => OnClickChoiceButton()).AddTo(_disposables);
            _ctx.OnAutoSelectButton.Subscribe(OnAutoSelectButton).AddTo(_disposables);
        }

        private void OnClickChoiceButton()
        {
           // interactable = false;
        }

        private void OnAutoSelectButton(int index)
        {
            if (_ctx.Index != index) return;

            OnButtonSelect();
            SetButtonState(true);
        }

        private void SetButtonState(bool toSelected)
        {
            defaultButton.gameObject.SetActive(!toSelected);
            hoverButton.gameObject.SetActive(toSelected);
        }

        public override void OnButtonSelect()
        {
            SetButtonState(true);
        }

        public override void OnButtonClick()
        {
        }

        public override void OnButtonNormal()
        {
            SetButtonState(false);
        }
    }
}