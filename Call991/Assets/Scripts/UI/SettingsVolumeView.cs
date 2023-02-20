using System;
using TMPro;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SettingsVolumeView : MenuButtonView
{
    [SerializeField] private Slider _slider = default;
    [SerializeField] private TextMeshProUGUI _sliderText = default;
    [SerializeField] private Image handleImage = default;
    [SerializeField] private AudioSourceType source = default;

    [SerializeField] [Range(0.01f, 0.5f)] private float step = 0.05f;
    private decimal _stepD;
    private ReactiveCommand<(AudioSourceType, float)> _onVolumeSet;

    private CompositeDisposable _disposables;

    public void Init(ReactiveCommand<(AudioSourceType, float)> onVolumeSet, float volume)
    {
        _disposables?.Dispose();

        _disposables = new CompositeDisposable();
        _stepD = new decimal(step);
        _onVolumeSet = onVolumeSet;
        _slider.value = volume;
        SetSliderText(volume);
        _slider.onValueChanged.AddListener(SetSliderValue);
    }

    public void Update()
    {
        if (InputHandler.GetPressLeft(gameObject))
            ChangeSliderValue(-step);

        if (InputHandler.GetPressRight(gameObject))
            ChangeSliderValue(step);
    }

    protected override void SetButtonState(bool hover)
    {
        base.SetButtonState(hover);
        var color = hover ? textHoverColor : textDefaultColor;
        handleImage.color = color;
        _sliderText.color = color;
    }

    private void ChangeSliderValue(float change)
    {
        _slider.value = Mathf.Clamp01(_slider.value + change);
    }

    private void SetSliderValue(float value)
    {
        // rounding the value to the nearest step
        value = (float) (Math.Round(new decimal(value) / _stepD) * _stepD);
        SetSliderText(value);
        _onVolumeSet?.Execute((source, value));
    }

    private void SetSliderText(float value)
    {
        _sliderText.text = ((int) (value * 100)).ToString();
    }

    protected override void OnDestroy()
    {
        _disposables?.Dispose();
        _slider.onValueChanged.RemoveAllListeners();
        base.OnDestroy();
    }
}