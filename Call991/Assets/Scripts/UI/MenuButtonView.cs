using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MenuButtonView : AaSelectable
    {
        [SerializeField] private Image defaultButton = default;
        [SerializeField] private Image hoverButton = default;
        
        [SerializeField] protected Color textDefaultColor = default;
        [SerializeField] protected Color textHoverColor = default;
        [SerializeField] protected Color textDisabledColor = default;
        [SerializeField] private TextMeshProUGUI text = default;

        public string Text
        {
            get => text.text;
            set => text.text = value;
        }

        public override void SetDisabled()
        {
            base.SetDisabled();

            defaultButton?.gameObject.SetActive(false);
            hoverButton?.gameObject.SetActive(false);
            text.color = textDisabledColor;
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
            
            defaultButton?.gameObject.SetActive(!toHover);
            hoverButton?.gameObject.SetActive(toHover);
            text.color = toHover ? textHoverColor : textDefaultColor;
        }
    }
}