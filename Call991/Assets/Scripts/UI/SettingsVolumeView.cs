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
    private AudioManager _audioManager;
    
    public void Init(AudioManager audioManager)
    {
        _audioManager = audioManager;

        _slider.onValueChanged.AddListener((v) => {
            _sliderText.text = ((int)(v * 100)).ToString();
            _audioManager.ChangeVolume(source, v);
        });
    }
    
    protected override void SetHoverColor(bool hover)
    {
        base.SetHoverColor(hover);
        var color = hover ? textHover : textNormal;
        handleImage.color = color;
        _sliderText.color = color;
    }

    protected override void OnDestroy()
    {
        _slider.onValueChanged.RemoveAllListeners();
        base.OnDestroy();
    }
}
