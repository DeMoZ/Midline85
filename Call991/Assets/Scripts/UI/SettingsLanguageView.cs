using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class SettingsLanguageView : MenuButtonView
{
    [SerializeField] private Button leftButton = default;
    [SerializeField] private Button rightButton = default;
    [SerializeField] private TextMeshProUGUI dropdownText = default;

    public void Update()
    {
        if (!_isSelected) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (leftButton.isActiveAndEnabled)
                leftButton.onClick?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (rightButton.isActiveAndEnabled)
                rightButton.onClick?.Invoke();
        }
    }

    protected override void SetHoverColor(bool hover)
    {
        base.SetHoverColor(hover);
        var color = hover ? textHover : textNormal;
        leftButton.image.color = color;
        rightButton.image.color = color;
        dropdownText.color = color;
    }
}