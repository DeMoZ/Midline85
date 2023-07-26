using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace UI
{
    public class LanguageDropdown : MonoBehaviour
    {
        [SerializeField] private TMP_Text localizedLabel = default;
        [SerializeField] private TMP_Dropdown dropdown = default;
        
        public TMP_Dropdown.DropdownEvent OnValueChanged => dropdown.onValueChanged;
        public List<TMP_Dropdown.OptionData> Options => dropdown.options;

        public void ClearOptions() => dropdown.ClearOptions();
        public void AddOptions(List<string> languages) => dropdown.AddOptions(languages);
        public void Value(int indexOf) => dropdown.value = indexOf;

        public void OnLeftButton()
        {
            var value = dropdown.value;
            value--;

            if (value < 0)
                value = dropdown.options.Count - 1;

            dropdown.value = value;
        }

        public void OnRightButton()
        {
            var value = dropdown.value;
            value++;

            if (value >= dropdown.options.Count)
                value = 0;

            dropdown.value = value;
        }

        private void Start()
        {
            LocalizeLabel();
            LocalizationManager.OnLocalizeEvent += LocalizeLabel;
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLocalizeEvent -= LocalizeLabel;
        }

        private void LocalizeLabel()
        {
            localizedLabel.text = LocalizationManager.GetTranslation(dropdown.options[dropdown.value].text);
        }
    }
}