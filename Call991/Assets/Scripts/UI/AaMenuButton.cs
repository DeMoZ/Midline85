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

        
        public override void OnButtonSelect()
        {
            throw new System.NotImplementedException();
        }

        public override void OnButtonClick()
        {
            throw new System.NotImplementedException();
        }

        public override void OnButtonNormal()
        {
            throw new System.NotImplementedException();
        }
    }
}