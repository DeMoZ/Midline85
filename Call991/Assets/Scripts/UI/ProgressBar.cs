using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image _bar;
    [SerializeField] private TMP_Text _value;
    [SerializeField] private TMP_Text _description;

    public void Set(float progress, string description)
    {
        Debug.Log($"{description} {progress}");
        _bar.fillAmount = progress;
        _value.text = $"{progress * 100:F2}/100";
        _description.text = string.IsNullOrEmpty(description)? "" : LocalizationManager.GetTranslation(description);
    }
}
