using TMPro;
using UnityEngine;

namespace UI
{
    public class MenuButtonView : AaSelectable
    {
        [SerializeField] private TextMeshProUGUI text = default;

        public string Text
        {
            get => text.text;
            set => text.text = value;
        }

        public override void SetDisabled()
        {
            base.SetDisabled();
            interactable = false;
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

        protected virtual void SetButtonState(bool toHover)
        {
            if (!interactable) return;
        }
    }
}