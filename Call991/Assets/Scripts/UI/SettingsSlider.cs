using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsSlider : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _sliderText;
    [SerializeField] private AudioSources _source;
    private AudioManager _audioManager;


    void Start()
    {
        
    }

    internal void Init(AudioManager audioManager)
    {
        _audioManager = audioManager;

        _slider.onValueChanged.AddListener((v) => {
            _sliderText.text = ((int)(v * 100)).ToString();
            _audioManager.ChangeVolume(_source, v);
        });
    }
}
