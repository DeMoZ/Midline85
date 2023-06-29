using System;
using I2.Loc;
using UniRx;
using UnityEngine;

[Serializable]
public class PlayerProfile : IDisposable
{
    private const string TextLanguageKey = "TextLanguage";
    private const string AudioLanguageKey = "AudioLanguage";
    private const string PlayerDataKey = "PlayerData";

    private const string DefaultTextLanguage = "English";
    //private const string DefaultAudioLanguage = "English";
    private const string DefaultAudioLanguage = "Russian";

    private string _textLanguage;
    private string _audioLanguage;

    private float _phraseVolume;
    private float _uiVolume;
    private float _timerVolume;
    private float _musicVolume;

    private CompositeDisposable _disposables;

    public ReactiveProperty<string> AudioLanguageChanged = new();
    public ReactiveCommand<(AudioSourceType type, float volume)> OnVolumeSet;

    public PlayerProfile()
    {
        _disposables = new CompositeDisposable();
        OnVolumeSet = new ReactiveCommand<(AudioSourceType type, float volume)>().AddTo(_disposables);
        LoadLanguages();
        LoadVolumes();
        OnVolumeSet.Subscribe(SaveVolume).AddTo(_disposables);
    }

    private void LoadLanguages()
    {
        var textLanguage = PlayerPrefs.GetString(TextLanguageKey, null);
        var audioLanguage = PlayerPrefs.GetString(AudioLanguageKey, null);

        var systemLanguage = Application.systemLanguage.ToString();
        var locLanguages = LocalizationManager.GetAllLanguages();

        if (string.IsNullOrEmpty(textLanguage))
            _textLanguage = locLanguages.Contains(systemLanguage) ? systemLanguage : DefaultTextLanguage;
        else
            _textLanguage = locLanguages.Contains(textLanguage) ? textLanguage : DefaultTextLanguage;

        if (string.IsNullOrEmpty(audioLanguage))
        {
            AudioLanguage = DefaultAudioLanguage;
            // TODO Uncomment
            // AudioLanguage = locLanguages.Contains(systemLanguage) ? systemLanguage : DefaultAudioLanguage;
        }
        else
            AudioLanguage = locLanguages.Contains(audioLanguage) ? audioLanguage : DefaultAudioLanguage;
    }

    private void LoadVolumes()
    {
        _phraseVolume = LoadVolume(AudioSourceType.Phrases);
        _uiVolume = LoadVolume(AudioSourceType.Effects);
        _timerVolume = LoadVolume(AudioSourceType.Effects);
        _musicVolume = LoadVolume(AudioSourceType.Music);

        // Debug.LogError($"on load: phrase = {_phraseVolume}; ui = {_uiVolume}; timer = {_timerVolume}; mus = {_musicVolume}");
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(PlayerDataKey);
    }

    public string TextLanguage
    {
        get => _textLanguage;
        set
        {
            _textLanguage = value;
            SaveLanguages();
        }
    }

    public string AudioLanguage
    {
        get => _audioLanguage;
        set
        {
            _audioLanguage = value;
            AudioLanguageChanged.Value = _audioLanguage;
            SaveLanguages();
        }
    }

    public float PhraseVolume
    {
        get => _phraseVolume;
        private set
        {
            _phraseVolume = value;
            SaveVolume(AudioSourceType.Phrases, value);
        }
    }

    public float UiVolume
    {
        get => _uiVolume;
        private set
        {
            _uiVolume = value;
            SaveVolume(AudioSourceType.Effects, value);
        }
    }

    public float TimerVolume
    {
        get => _timerVolume;
        private set
        {
            _timerVolume = value;
            SaveVolume(AudioSourceType.Effects, value);
        }
    }

    public float MusicVolume
    {
        get => _musicVolume;
        private set
        {
            _musicVolume = value;
            SaveVolume(AudioSourceType.Music, value);
        }
    }

    private void SaveVolume((AudioSourceType type, float volume) value)
    {
        switch (value.type)
        {
            case AudioSourceType.Phrases:
                PhraseVolume = value.volume;
                break;
            case AudioSourceType.Effects:
                UiVolume = value.volume;
                TimerVolume = value.volume;
                break;
            case AudioSourceType.Music:
                MusicVolume = value.volume;
                break;
        }
    }

    private void SaveLanguages()
    {
        PlayerPrefs.SetString(TextLanguageKey, _textLanguage);
        PlayerPrefs.SetString(AudioLanguageKey, _audioLanguage);
    }

    private void SaveVolume(AudioSourceType type, float volume)
    {
        PlayerPrefs.SetFloat(type.ToString(), volume);
        //onVolumeChanged?.Execute((type, volume)); - if only need additional calculation
    }

    private float LoadVolume(AudioSourceType sourceType) =>
        PlayerPrefs.GetFloat(sourceType.ToString(), 1);

#if UNITY_EDITOR
    public void SaveLanguages(string textLanguage, string audioLanguage)
    {
        PlayerPrefs.SetString(TextLanguageKey, textLanguage.ToString());
        PlayerPrefs.SetString(AudioLanguageKey, audioLanguage.ToString());
    }
#endif
    public void Dispose()
    {
        _disposables?.Dispose();
    }
}