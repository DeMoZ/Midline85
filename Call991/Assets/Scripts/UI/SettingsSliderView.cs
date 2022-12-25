using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class SettingsSliderView : MenuButtonView
{
    [SerializeField] private Button leftButton = default;
    [SerializeField] private Button rightButton = default;
    [SerializeField] private TextMeshProUGUI dropdownText = default;
    protected override void SetHoverColor(bool hover)
    {
        base.SetHoverColor(hover);
        var color = hover ? textHover : textNormal;
        leftButton.image.color = color;
        rightButton.image.color = color;
        dropdownText.color = color;
    }
}
