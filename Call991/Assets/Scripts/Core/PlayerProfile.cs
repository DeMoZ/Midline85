using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    private const string ProfileKey = "Profile";
    private Data _data;

    public Data Data => _data;

    public PlayerProfile()
    {
        var savedProfile = PlayerPrefs.GetString(ProfileKey, null);
        _data = string.IsNullOrWhiteSpace(savedProfile) ? 
            new Data() 
            : JsonConvert.DeserializeObject<Data>(savedProfile);
    }
    
    // public void Load()
    // {
    //     var savedProfile = PlayerPrefs.GetString(ProfileKey, null);
    //     _data = string.IsNullOrWhiteSpace(savedProfile) ? 
    //         new Data() 
    //         : JsonConvert.DeserializeObject<Data>(savedProfile);
    // }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(ProfileKey);
        _data = new Data();
    }

    public void SetLastPhrase(string phraseId)
    {
        _data.lastPhraseId = phraseId;
        SaveToPrefs();
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

    private void SaveToPrefs() => 
        PlayerPrefs.SetString(ProfileKey, JsonConvert.SerializeObject(_data));
}

public class Data
{
    public string lastPhraseId = null;
    public List<string> choices = new List<string>();
    public List<string> phrases = new List<string>();
}