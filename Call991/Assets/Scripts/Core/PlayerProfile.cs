using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    private const string ProfileKey = "Profile";
    private PlayerData _playerData;
    
    public PlayerProfile()
    {
        var savedProfile = PlayerPrefs.GetString(ProfileKey, null);
        _playerData = string.IsNullOrWhiteSpace(savedProfile)
            ? new PlayerData()
            : JsonConvert.DeserializeObject<PlayerData>(savedProfile);
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(ProfileKey);
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
    
    public string LastPhrase
    {
        get => _playerData.lastPhraseId;
        set
        {
            _playerData.lastPhraseId = value;
            AddPhrase(value);
            SaveToPrefs();
        }
    }

    public string CheatPhrase
    {
        get => _playerData.cheatPhraseId;
        set
        {
            _playerData.cheatPhraseId = value;
            SaveToPrefs();
        }
    }

    public void AddPhrase(string phraseId)
    {
        if (_playerData.phrases.Contains(phraseId)) return;
        
        _playerData.phrases.Add(phraseId);
        SaveToPrefs();
    }

    public void AddChoice(string choiceId)
    {
        if (_playerData.choices.Contains(choiceId)) return;
        
        _playerData.choices.Add(choiceId);
        SaveToPrefs();
    }

    public bool ContainsChoice(string choiceId) => 
        _playerData.choices.Contains(choiceId);

    public bool ContainsChoice(List<string> choices)
    {
        foreach (var required in choices)
        {
            if (!ContainsChoice(required)) ;
                return false;
        }

        return true;
    }
    public bool ContainsPhrase(string phraseId) => 
        _playerData.phrases.Contains(phraseId);

    private void SaveToPrefs() =>
        PlayerPrefs.SetString(ProfileKey, JsonConvert.SerializeObject(_playerData));
}

public class PlayerData
{
    public string lastPhraseId = null;
    public string cheatPhraseId = null;

    public List<string> choices = new List<string>();
    public List<string> phrases = new List<string>();
}