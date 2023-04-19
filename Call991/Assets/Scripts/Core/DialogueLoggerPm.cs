using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AaDialogueGraph;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Write all the nodes the player have seen into cash and prefs
/// </summary>
public class DialogueLoggerPm : IDisposable
{
    private const string LOGKey = "LogNodes";
    private const string ChoicesKey = "Choices";
    private const string EndsKey = "Ends";
    private const string CountsKey = "Counts";

    private Dictionary<string, string> _logCash;
    private Dictionary<string, string> _choicesCash;
    private Dictionary<string, string> _endsCash;
    private Dictionary<string, int> _countsCash;

    public DialogueLoggerPm()
    {
        Load();
    }

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
        Debug.LogError($"[{this}] Add Choice {choice}");
        _choicesCash.Add(choice, "");
    }

    private void AddEnd(string end)
    {
        Debug.LogError($"[{this}] Add Choice {end}");
        _endsCash.Add(end, "");
    }

    public bool ContainsChoice(List<string> orChoices)
    {
        return orChoices.Any(orChoice => _choicesCash.ContainsKey(orChoice));
    }

    public bool ContainsEnd(List<string> orEnds)
    {
        return orEnds.Any(orEnd => _endsCash.ContainsKey(orEnd));
    }

    public void AddCount(string key, int value)
    {
        Debug.LogError($"[{this}] Add Count {key}:{value}");
        _countsCash.TryGetValue(key, out var count);
        _countsCash[key] = count + value;
    }

    public int GetCount(string key)
    {
        _countsCash.TryGetValue(key, out var count);
        return count;
    }

    private void AddNode(string key, string value)
    {
        _logCash.Add(key, value);
    }

    public void Save()
    {
        SetToPrefs(LOGKey, _logCash);
        SetToPrefs(ChoicesKey, _choicesCash);
        SetToPrefs(EndsKey, _endsCash);
        SetToPrefs(CountsKey, _countsCash);
    }

    private void Load()
    {
        _logCash = GetFromPrefs<string>(LOGKey);
        _choicesCash = GetFromPrefs<string>(ChoicesKey);
        _endsCash = GetFromPrefs<string>(EndsKey);
        _countsCash = GetFromPrefs<int>(CountsKey);
    }

    private Dictionary<string, T> GetFromPrefs<T>(string prefsKey)
    {
        var prefsData = PlayerPrefs.GetString(prefsKey, null);

        return !string.IsNullOrEmpty(prefsData)
            ? JsonConvert.DeserializeObject<Dictionary<string, T>>(prefsData)
            : new Dictionary<string, T>();
    }

    private void SetToPrefs<T>(string key, T data)
    {
        var serialized = JsonConvert.SerializeObject(data);
        PlayerPrefs.SetString(key, serialized);
    }

    public void Dispose()
    {
    }
}