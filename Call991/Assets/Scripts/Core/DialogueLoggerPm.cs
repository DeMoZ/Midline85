using System;
using System.Collections.Generic;
using AaDialogueGraph;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Write all the nodes the player have seen into prefs
/// </summary>
public class DialogueLoggerPm : IDisposable
{
    private const string LOGKey = "LogNodes";
    private const string ChoicesKey = "Choices";

    private Dictionary<string, string> _logCash = new();
    private Dictionary<string, string> _choicesCash = new();

    public void AddLog(List<AaNodeData> datas)
    {
        foreach (var data in datas)
        {
            AddLog(data);
        }
    }
    
    public void AddLog(AaNodeData data)
    {
        switch (data)
        {
            case PhraseNodeData nodeData:
            {
                AddNode(nodeData.Guid, nodeData.PhraseSketchText);
                break;
            }
            case ChoiceNodeData nodeData:
            {
                AddChoice(nodeData.Choice);
                AddNode(nodeData.Guid, nodeData.Choice);
                break;
            }
            case CountNodeData nodeData:
            {
                AddNode(nodeData.Guid, nodeData.Value.ToString());
                break;
            }
        }
    }

    private void AddChoice(string choice)
    {
        var deserializedData = new Dictionary<string, string>();
        var serializedData = PlayerPrefs.GetString(ChoicesKey, null);

        if (!string.IsNullOrEmpty(serializedData))
        {
            deserializedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedData);
        }

        deserializedData[choice] = string.Empty;
        serializedData = JsonConvert.SerializeObject(deserializedData);
        PlayerPrefs.SetString(ChoicesKey, serializedData);
        Debug.LogWarning($"[{this}] save one more choice {choice};" +
                         $"\n{serializedData}");
    }

    public bool ContainsChoice(string choice)
    {
        var deserializedData = new Dictionary<string, string>();
        var serializedData = PlayerPrefs.GetString(ChoicesKey, null);

        if (!string.IsNullOrEmpty(serializedData))
        {
            deserializedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedData);
        }

        return deserializedData.TryGetValue(choice, out var data);
    }
    
    public void AddCount(string key, int value)
    {
        var count = PlayerPrefs.GetInt(key, 0);
        count += value;
        PlayerPrefs.SetInt(key, count);
    }

    public int GetCount(string key)
    {
        return PlayerPrefs.GetInt(key, 0);
    }
    
    private void AddNode(string key, string value)
    {
        var deserializedData = new Dictionary<string, string>();
        var serializedData = PlayerPrefs.GetString(LOGKey, null);

        if (!string.IsNullOrEmpty(serializedData))
        {
            deserializedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedData);
        }

        if(deserializedData.TryGetValue(key, out var nodeData))
        {
            deserializedData[key] = value;
            serializedData = JsonConvert.SerializeObject(deserializedData);
        }
        
        PlayerPrefs.SetString(LOGKey,serializedData);
    }
    
    public void Dispose()
    {
    }
}