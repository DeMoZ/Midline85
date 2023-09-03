using UI;
using UnityEngine;
using UnityEngine.UI;

public class AaLanguageSlider : AaButton
{
    [SerializeField] private Button leftButton = default;
    [SerializeField] private Button rightButton = default;
    
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

    protected override void OnButtonSelect()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnButtonClick()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnButtonNormal()
    {
        throw new System.NotImplementedException();
    }
}