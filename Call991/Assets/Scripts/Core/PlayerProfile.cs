using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    private const string ProfileKey = "Profile";
    
    private Data data;

    public Data Data => data;

    public void Load()
    {
        var savedProfile = PlayerPrefs.GetString(ProfileKey, null);
        data = string.IsNullOrWhiteSpace(savedProfile) ? 
            new Data() 
            : JsonConvert.DeserializeObject<Data>(savedProfile);
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(ProfileKey);
        data = new Data();
    }

    public void AddPhrase(string phraseId) =>
        data.phrases.Add(phraseId);
    
    public void AddChoice(string choiceId) => 
        data.choices.Add(choiceId);
}

public class Data
{
    public string lastPhraseId = null;
    public List<string> choices = new List<string>();
    public List<string> phrases = new List<string>();
}