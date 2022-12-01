using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsSlider : MonoBehaviour
{
    [SerializeField] private Slider _slider = default;
    [SerializeField] private TextMeshProUGUI _sliderText = default;
    [SerializeField] private AudioSourceType _source = default;
    private AudioManager _audioManager;

    public void Init(AudioManager audioManager)
    {
        _audioManager = audioManager;

        _slider.onValueChanged.AddListener((v) => {
            _sliderText.text = ((int)(v * 100)).ToString();
            _audioManager.ChangeVolume(_source, v);
        });
    }
}
