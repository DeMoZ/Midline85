using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

[Serializable]
public class PlayerProfile : IDisposable
{
    private const string TextLanguageKey = "TextLanguage";
    private const string AudioLanguageKey = "AudioLanguage";
    private const string PlayerDataKey = "PlayerData";
   
    private string _textLanguage;
    private string _audioLanguage;

    private float _phraseVolume;
    private float _uiVolume;
    private float _timerVolume;
    private float _musicVolume;

    private PlayerData _playerData;
    private ReactiveCommand<string> _onAudioLanguage;
    private CompositeDisposable _disposables;

    public ReactiveCommand<(AudioSourceType type, float volume)> onVolumeSet;

    public PlayerProfile(ReactiveCommand<string> onAudioLanguage)
    {
        _disposables = new CompositeDisposable();
        
        _onAudioLanguage = onAudioLanguage;
        var defaultLanguage = LocalizationManager.CurrentLanguage;
        _textLanguage = PlayerPrefs.GetString(TextLanguageKey, defaultLanguage);
        _audioLanguage = PlayerPrefs.GetString(AudioLanguageKey, defaultLanguage);

        _onAudioLanguage?.Execute(_audioLanguage);

        var savedProfile = PlayerPrefs.GetString(PlayerDataKey, null);
        _playerData = string.IsNullOrWhiteSpace(savedProfile)
            ? new PlayerData()
            : JsonConvert.DeserializeObject<PlayerData>(savedProfile);

        onVolumeSet = new ReactiveCommand<(AudioSourceType type, float volume)>();
        LoadVolumes();
        onVolumeSet.Subscribe(SaveVolume).AddTo(_disposables);
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
        _playerData = new PlayerData();
    }

    public void ClearPhrases()
    {
        _playerData.phrases.Clear();
    }

    public void ClearChoices()
    {
        _playerData.choices.Clear();
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

    public float UiVolume{
        get => _uiVolume;
        private set
        {
            _uiVolume = value;
            SaveVolume(AudioSourceType.Effects, value);
        }
    }
    
    public float TimerVolume {
        get => _timerVolume;
        private set
        {
            _timerVolume = value;
            SaveVolume(AudioSourceType.Effects, value);
        }
    }
    
    public float MusicVolume {
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
    
    public string LastPhrase
    {
        get => _playerData.lastPhraseId;
        set
        {
            _playerData.lastPhraseId = value;
            AddPhrase(value);
            SavePlayerData();
        }
    }

    public string CheatPhrase
    {
        get => _playerData.cheatPhraseId;
        set
        {
            _playerData.cheatPhraseId = value;
            SavePlayerData();
        }
    }

    public void AddPhrase(string phraseId)
    {
        if (_playerData.phrases.Contains(phraseId)) return;

        _playerData.phrases.Add(phraseId);
        SavePlayerData();
    }

    public void AddChoice(string choiceId)
    {
        if (_playerData.choices.Contains(choiceId)) return;

        _playerData.choices.Add(choiceId);
        SavePlayerData();
    }

    public bool ContainsChoice(List<string> choices)
    {
        foreach (var required in choices)
        {
            var orChoices = required.Split('|');

            if (!_playerData.choices.Any(orChoices.Contains))
                return false;
        }

        return true;
    }

    public PlayerData GetPlayerData() =>
        _playerData;

    private void SavePlayerData() =>
        PlayerPrefs.SetString(PlayerDataKey, JsonConvert.SerializeObject(_playerData));

    private void SaveLanguages()
    {
        PlayerPrefs.SetString(TextLanguageKey, _textLanguage);
        PlayerPrefs.SetString(AudioLanguageKey, _audioLanguage);

        _onAudioLanguage?.Execute(_audioLanguage);
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
        _disposables.Dispose();
        
        _onAudioLanguage?.Dispose();
        onVolumeSet?.Dispose();
        _disposables?.Dispose();
    }
}

public class PlayerData
{
    public string lastPhraseId = null;
    public string cheatPhraseId = null;

    public List<string> choices = new List<string>();
    public List<string> phrases = new List<string>();
}