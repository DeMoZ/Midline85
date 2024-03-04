using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AaMenuProgressButton : AaButton
    {
        [SerializeField] private TextMeshProUGUI text = default;
        [SerializeField] private Image _progressImage;

        private void Awake()
        {
            EnableProgress(false);
        }

        public void EnableProgress(bool enable)
        {
            SetProgress(0);
            _progressImage.gameObject.SetActive(enable);    
        }
        
        public void SetProgress(float value)
        {
            _progressImage.fillAmount = value;
        }
        
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