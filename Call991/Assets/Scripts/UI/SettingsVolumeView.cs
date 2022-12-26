using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class SettingsVolumeView : MenuButtonView
{
    [SerializeField] private Slider _slider = default;
    [SerializeField] private TextMeshProUGUI _sliderText = default;
    [SerializeField] private Image handleImage = default;
    [SerializeField] private AudioSourceType source = default;

    [SerializeField] [Range(0.01f, 0.5f)] private float step = 0.05f;

    private AudioManager _audioManager;

    public void Init(AudioManager audioManager)
    {
        _audioManager = audioManager;
        _slider.onValueChanged.AddListener(SetSliderValue);
    }

    public void Update()
    {
        if (!_isSelected) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            ChangeSliderValue(-step);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            ChangeSliderValue(step);
    }

    protected override void SetHoverColor(bool hover)
    {
        base.SetHoverColor(hover);
        var color = hover ? textHover : textNormal;
        handleImage.color = color;
        _sliderText.color = color;
    }
    
    private void ChangeSliderValue(float change)
    {
        _slider.value = Mathf.Clamp01(_slider.value + change);
    }

    private void SetSliderValue(float value)
    {
        _sliderText.text = ((int) (value * 100)).ToString();
        _audioManager.ChangeVolume(source, value);
    }

    protected override void OnDestroy()
    {
        _slider.onValueChanged.RemoveAllListeners();
        base.OnDestroy();
    }
}