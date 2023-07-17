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

    private float _masterVolume;
    private float _voiceVolume;
    private float _musicVolume;
    private float _sfxVolume;

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
        _masterVolume = LoadVolume(AudioSourceType.Master);
        _voiceVolume = LoadVolume(AudioSourceType.Voice);
        _musicVolume = LoadVolume(AudioSourceType.Music);
        _sfxVolume = LoadVolume(AudioSourceType.Sfx);

        Debug.Log($"<color=yellow>volumes on load:</color> master = {_masterVolume}; voice = {_voiceVolume}; " +
                  $"music = {_musicVolume}; sfx = {_sfxVolume};");
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

    public float MasterVolume
    {
        get => _masterVolume;
        private set
        {
            _masterVolume = value;
            SaveVolume(AudioSourceType.Master, value);
        }
    }

    public float VoiceVolume
    {
        get => _voiceVolume;
        private set
        {
            _voiceVolume = value;
            SaveVolume(AudioSourceType.Voice, value);
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

    public float SfxVolume
    {
        get => _sfxVolume;
        private set
        {
            _sfxVolume = value;
            SaveVolume(AudioSourceType.Sfx, value);
        }
    }

    private void SaveVolume((AudioSourceType type, float volume) value)
    {
        switch (value.type)
        {
            case AudioSourceType.Master:
                MasterVolume = value.volume;
                break;
            case AudioSourceType.Voice:
                VoiceVolume = value.volume;
                break;
            case AudioSourceType.Music:
                MusicVolume = value.volume;
                break;
            case AudioSourceType.Sfx:
                SfxVolume = value.volume;
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
        PlayerPrefs.GetFloat(sourceType.ToString(), 100);

#if UNITY_EDITOR
    public void SaveLanguages(string textLanguage, string audioLanguage)
    {
        PlayerPrefs.SetString(TextLanguageKey, textLanguage);
        PlayerPrefs.SetString(AudioLanguageKey, audioLanguage);
    }
#endif
    public void Dispose()
    {
        _disposables?.Dispose();
    }
}