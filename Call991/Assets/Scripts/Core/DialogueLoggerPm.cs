using System;
using System.Collections.Generic;
using System.Linq;
using AaDialogueGraph;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Write all the nodes the player have seen into cash and prefs
/// </summary>
public class DialogueLoggerPm : IDisposable
{
    private const string GameProgress = "GameProgress";

    private Dictionary<string, string> _logCash;
    private Dictionary<string, string> _choicesCash;
    private Dictionary<string, string> _endsCash;
    private Dictionary<string, int> _countsCash;

    private string _currentLevelId;

    // all progress loaded from prefs (to modify and save it back)
    private GameContainer _savedCash;

    //  
    private Dictionary<string, string> _savedChoices;
    private Dictionary<string, string> _savedEnds;
    private Dictionary<string, int> _savedCounts;
    private bool _isIntitialized;

    public DialogueLoggerPm()
    {
       
    }

    public void Init(string levelId)
    {
        _isIntitialized = true;
        _currentLevelId = levelId;
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
                AddNode(nodeData.Guid, nodeData.PhraseSketchText);
                break;
            case ChoiceNodeData nodeData:
                AddCaseInCash(nodeData.Choice, _choicesCash);
                AddNode(nodeData.Guid, nodeData.Choice);
                break;
            case CountNodeData nodeData:
                AddNode(nodeData.Guid, nodeData.Value.ToString());
                break;
            case EndNodeData nodeData:
                AddCaseInCash(nodeData.End, _endsCash);
                AddNode(nodeData.Guid, nodeData.End);
                break;
            case EventNodeData:
            case NewspaperNodeData:
                AddNode(data.Guid, "");
                break;
        }
    }

    private void AddCaseInCash(string condition, IDictionary<string, string> dictionary)
    {
        Debug.Log($"[{this}] Add case into cash {condition}");
        dictionary[condition] = "";
    }

    public bool ContainsChoice(List<string> orChoices) =>
        orChoices.Any(orChoice => _choicesCash.ContainsKey(orChoice) ||
                                  _savedChoices.ContainsKey(orChoice));

    public bool ContainsEnd(List<string> orEnds) =>
        orEnds.Any(orEnd => _endsCash.ContainsKey(orEnd) ||
                            _savedEnds.ContainsKey(orEnd));

    public void AddCount(string key, int value)
    {
        Debug.Log($"[{this}] Add Count {key}:{value}");
        _countsCash.TryGetValue(key, out var count);
        _countsCash[key] = count + value;
    }

    public int GetCount(string key)
    {
        _countsCash.TryGetValue(key, out var countCurrent);
        _savedCounts.TryGetValue(key, out var countSaved);
        return countCurrent + countSaved;
    }

    private void AddNode(string key, string value) =>
        _logCash[key] = value;

    public void Save()
    {
        _savedCash.LevelProgress[_currentLevelId] = new LevelContainer
        {
            LogCash = _logCash,
            ChoicesCash = _choicesCash,
            EndsCash = _endsCash,
            CountsCash = _countsCash,
        };

        var data = JsonConvert.SerializeObject(_savedCash);
        PlayerPrefs.SetString(GameProgress, data);
        Debug.Log($"[{this}] -- <color=green>Progress saved</color> -- {_currentLevelId}");
    }

    public List<LevelInfo> LoadLevelsInfo()
    {
        List<LevelInfo> info = new List<LevelInfo>();
        
        var stringData = PlayerPrefs.GetString(GameProgress, string.Empty);
        var cash = string.IsNullOrEmpty(stringData)
            ? new GameContainer()
            : JsonConvert.DeserializeObject<GameContainer>(stringData);

        foreach (var level in cash.LevelProgress)
        {
            var hasRecord = level.Value.EndsCash.Count > 0;
            info.Add(new LevelInfo
            {
                Key = level.Key,
                HasRecord = hasRecord,
            });
            
            //-----
            // Debug.LogWarning($"Level {level.Key}");
            // Debug.LogWarning($"Has record {hasRecord}");
            // foreach (var record in level.Value.EndsCash)
            // {
            //     Debug.LogWarning($"Record {record.Key}; {record.Value}");    
            // }
        }

        return info;
    }
    
    private void Load()
    {
        //_savedCash.LevelProgress.Clear();

        _logCash = new Dictionary<string, string>();
        _choicesCash = new Dictionary<string, string>();
        _endsCash = new Dictionary<string, string>();
        _countsCash = new Dictionary<string, int>();

        _savedChoices = new Dictionary<string, string>();
        _savedEnds = new Dictionary<string, string>();
        _savedCounts = new Dictionary<string, int>();

        var stringData = PlayerPrefs.GetString(GameProgress, string.Empty);
        _savedCash = string.IsNullOrEmpty(stringData)
            ? new GameContainer()
            : JsonConvert.DeserializeObject<GameContainer>(stringData);

        // reset cashed data for current level to not affect current level progress
        _savedCash.LevelProgress[_currentLevelId] = new LevelContainer();

        foreach (var level in _savedCash.LevelProgress)
        {
            foreach (var pair in level.Value.ChoicesCash) _savedChoices[pair.Key] = pair.Value;
            foreach (var pair in level.Value.EndsCash) _savedEnds[pair.Key] = pair.Value;
            foreach (var pair in level.Value.CountsCash) _savedCounts[pair.Key] = pair.Value;
        }
        Debug.Log($"[{this}] -- <color=yellow>Progress loaded</color>");
    }

    public void Dispose()
    {
        if (!_isIntitialized) return; 
        
        // Save();
    }
}

[Serializable]
public class GameContainer
{
    public Dictionary<string, LevelContainer> LevelProgress = new();
}

[Serializable]
public class LevelContainer
{
    public Dictionary<string, string> LogCash = new();
    public Dictionary<string, string> ChoicesCash = new();
    public Dictionary<string, string> EndsCash = new();
    public Dictionary<string, int> CountsCash = new();
}

public class LevelInfo
{
    public string Key;
    public bool HasRecord;
}