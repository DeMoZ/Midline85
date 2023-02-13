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
        [SerializeField] private TextMeshProUGUI text = default;

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
        }

        protected virtual void SetButtonState(bool toHover)
        {
            defaultButton?.gameObject.SetActive(!toHover);
            hoverButton?.gameObject.SetActive(toHover);
            text.color = toHover ? textHoverColor : textDefaultColor;
        }
    }
}