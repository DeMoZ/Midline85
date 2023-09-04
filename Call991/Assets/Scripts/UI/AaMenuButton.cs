using TMPro;
using UnityEngine;

namespace UI
{
    public class AaMenuButton : AaButton
    {
        [SerializeField] private TextMeshProUGUI text = default;
        
        public string Text
        {
            get => text.text;
            set => text.text = value;
        }


        protected override void OnButtonSelect()
        {
        }

        protected override void OnButtonClick()
        {
        }

        protected override void OnButtonNormal()
        {
        }
    }
}