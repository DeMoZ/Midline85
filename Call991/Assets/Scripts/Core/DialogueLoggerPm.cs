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
    private readonly LoggerService _loggerService;
    private string _currentLevelId;

    // all progress loaded from prefs (to modify and save it back)
    private GameContainer _gameContainer;

    //  
    private Dictionary<string, string> _savedChoices;
    private Dictionary<string, string> _savedEnds;
    private Dictionary<string, int> _savedCounts;
    private bool _isInitialized;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loggerService"></param>
    /// <param name="gameContainer">All game levels data</param>
    /// <param name="levelContainer">Current level data</param>
    public DialogueLoggerPm(LoggerService loggerService, GameContainer gameContainer = null)
    {
        _loggerService = loggerService;
        _gameContainer = gameContainer;
    }

    public void Init(string levelId)
    {
        _isInitialized = true;
        _currentLevelId = levelId;
        Load();
    }

    public bool ContainsChoice(List<string> orChoices) =>
        orChoices.Any(orChoice => _loggerService.LevelContainer.ChoicesCash.ContainsKey(orChoice) ||
                                  _savedChoices.ContainsKey(orChoice));

    public bool ContainsEnd(List<string> orEnds) =>
        orEnds.Any(orEnd => _loggerService.LevelContainer.EndsCash.ContainsKey(orEnd) ||
                            _savedEnds.ContainsKey(orEnd));

    public void AddCount(string key, int value)
    {
        Debug.Log($"[{this}] Add Count {key}:{value}");
        _loggerService.LevelContainer.CountsCash.TryGetValue(key, out var count);
        _loggerService.LevelContainer.CountsCash[key] = count + value;
    }

    public int GetCount(string key)
    {
        _loggerService.LevelContainer.CountsCash.TryGetValue(key, out var countCurrent);
        _savedCounts.TryGetValue(key, out var countSaved);
        return countCurrent + countSaved;
    }

    public void Save()
    {
        _gameContainer.LevelProgress[_currentLevelId] = _loggerService.LevelContainer;
        var data = JsonConvert.SerializeObject(_gameContainer);
        PlayerPrefs.SetString(AaConstants.GameProgress, data);
        Debug.Log($"[{this}] -- <color=green>Progress saved</color> -- {_currentLevelId}");
    }

    public List<LevelInfo> LoadLevelsInfo()
    {
        List<LevelInfo> info = new List<LevelInfo>();

        var stringData = PlayerPrefs.GetString(AaConstants.GameProgress, string.Empty);
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
        _loggerService.Clear();

        _savedChoices = new Dictionary<string, string>();
        _savedEnds = new Dictionary<string, string>();
        _savedCounts = new Dictionary<string, int>();

        var stringData = PlayerPrefs.GetString(AaConstants.GameProgress, string.Empty);
        _gameContainer = string.IsNullOrEmpty(stringData)
            ? new GameContainer()
            : JsonConvert.DeserializeObject<GameContainer>(stringData);

        // reset cashed data for current level to not affect current level progress
        _gameContainer.LevelProgress[_currentLevelId] = new LevelContainer();

        foreach (var level in _gameContainer.LevelProgress)
        {
            foreach (var pair in level.Value.ChoicesCash) _savedChoices[pair.Key] = pair.Value;
            foreach (var pair in level.Value.EndsCash) _savedEnds[pair.Key] = pair.Value;
            foreach (var pair in level.Value.CountsCash) _savedCounts[pair.Key] = pair.Value;
        }

        Debug.Log($"[{this}] -- <color=yellow>Progress loaded</color>");
    }

    public void Dispose()
    {
        if (!_isInitialized) return;

        // Save();
    }

    public void AddLog(List<AaNodeData> data) => _loggerService.AddLog(data);
    public void AddLog(CountNodeData data) => _loggerService.AddLog(data);
    public void AddLog(EndNodeData data) => _loggerService.AddLog(data);
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