using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    private const string ProfileKey = "Profile";
    private Data _data;
    
    public PlayerProfile()
    {
        var savedProfile = PlayerPrefs.GetString(ProfileKey, null);
        _data = string.IsNullOrWhiteSpace(savedProfile)
            ? new Data()
            : JsonConvert.DeserializeObject<Data>(savedProfile);
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(ProfileKey);
        _data = new Data();
    }

    public string LastPhrase
    {
        get => _data.lastPhraseId;
        set
        {
            _data.lastPhraseId = value;
            SaveToPrefs();
        }
    }

    public string CheatPhrase
    {
        get => _data.cheatPhraseId;
        set
        {
            _data.cheatPhraseId = value;
            SaveToPrefs();
        }
    }

    public void AddPhrase(string phraseId)
    {
        _data.phrases.Add(phraseId);
        SaveToPrefs();
    }

    public void AddChoice(string choiceId)
    {
        _data.choices.Add(choiceId);
        SaveToPrefs();
    }

    public bool ContainsChoice(string choiceId) => 
        _data.choices.Contains(choiceId);

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
        _data.phrases.Contains(phraseId);

    private void SaveToPrefs() =>
        PlayerPrefs.SetString(ProfileKey, JsonConvert.SerializeObject(_data));
}

public class Data
{
    public string lastPhraseId = null;
    public string cheatPhraseId = null;

    public List<string> choices = new List<string>();
    public List<string> phrases = new List<string>();
}