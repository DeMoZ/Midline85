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
        if (InputHandler.GetPressLeft(gameObject))
        {
            if (leftButton.isActiveAndEnabled)
                leftButton.onClick?.Invoke();
        }

        if (InputHandler.GetPressRight(gameObject))
        {
            if (rightButton.isActiveAndEnabled)
                rightButton.onClick?.Invoke();
        }
    }
}