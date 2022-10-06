using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    private const string TextLanguageKey = "TextLanguage";
    private const string AudioLanguageKey = "AudioLanguage";
    private const string PlayerDataKey = "PlayerData";

    private Language _textLanguage;
    private Language _audioLanguage;

    private PlayerData _playerData;
    private ReactiveCommand<Language> _onAudioLanguage;

    public PlayerProfile(ReactiveCommand<Language> onAudioLanguage)
    {
        _onAudioLanguage = onAudioLanguage;

        var textLanguage = PlayerPrefs.GetString(TextLanguageKey, Language.EN.ToString());
        Enum.TryParse(textLanguage, out _textLanguage);

        //var audioLanguage = PlayerPrefs.GetString(AudioLanguageKey, Language.EN.ToString());
        var audioLanguage = PlayerPrefs.GetString(AudioLanguageKey, Language.RU.ToString());
        Enum.TryParse(audioLanguage, out _audioLanguage);

        _onAudioLanguage?.Execute(_audioLanguage);

        var savedProfile = PlayerPrefs.GetString(PlayerDataKey, null);
        _playerData = string.IsNullOrWhiteSpace(savedProfile)
            ? new PlayerData()
            : JsonConvert.DeserializeObject<PlayerData>(savedProfile);
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

    public Language TextLanguage
    {
        get => _textLanguage;
        set
        {
            _textLanguage = value;
            SaveLanguages();
        }
    }

    public Language AudioLanguage
    {
        get => _audioLanguage;
        set
        {
            _audioLanguage = value;
            SaveLanguages();
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
        PlayerPrefs.SetString(TextLanguageKey, _textLanguage.ToString());
        PlayerPrefs.SetString(AudioLanguageKey, _audioLanguage.ToString());

        _onAudioLanguage?.Execute(_audioLanguage);
    }

#if UNITY_EDITOR
    public void SaveLanguages(Language textLanguage, Language audioLanguage)
    {
        PlayerPrefs.SetString(TextLanguageKey, textLanguage.ToString());
        PlayerPrefs.SetString(AudioLanguageKey, audioLanguage.ToString());
    }
#endif
}

public class PlayerData
{
    public string lastPhraseId = null;
    public string cheatPhraseId = null;

    public List<string> choices = new List<string>();
    public List<string> phrases = new List<string>();
}