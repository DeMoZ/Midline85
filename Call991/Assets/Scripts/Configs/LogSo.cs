using System;
using System.Collections.Generic;
using System.Linq;
using AaDialogueGraph;
using I2.Loc;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LogSo), menuName = "Aa/Configs/" + nameof(LogSo), order = 0)]
public class LogSo : SerializedScriptableObject
{
    [SerializeField] private List<DialogueLog> dialogueLogs;

    [Button]
    [HorizontalGroup("PrefsControl")]
    public void ClearPrefs()
    {
        PlayerPrefs.DeleteKey(AaConstants.GameProgress);
    }

    [Button]
    [HorizontalGroup("PrefsControl")]
    public void SaveGameToPrefs()
    {
        var loggerService = new LoggerService();

        // levels logs to game container
        var gameContainer = new GameContainer();
        foreach (var dialogue in dialogueLogs)
        {
            loggerService.AddLog(dialogue.AllNodesData);
            var entryNode = dialogue.Dialogue.EntryNodeData;
            gameContainer.LevelProgress[entryNode.LevelId] = loggerService.LevelContainer;
        }
        
        var data = JsonConvert.SerializeObject(gameContainer);
        PlayerPrefs.SetString(AaConstants.GameProgress, data);
        Debug.Log($"[{this}] -- <color=green>Progress saved</color> -- for all levels in LogSo");
    }
}

[Serializable]
public class DialogueLog
{
    [SerializeField] private DialogueContainer dialogue;
    [SerializeField] private bool showOnlyButtons = true;
    private bool ShowAllLogs => !showOnlyButtons;

    public List<AaNodeData> AllNodesData => _allNodesData;
    public DialogueContainer Dialogue => dialogue;

    [ReadOnly, ShowIf("ShowAllLogs")] public List<LogUnit> allLog = new();
    [ReadOnly, ShowIf("showOnlyButtons")] public List<LogUnit> choiceLog = new();

    private List<AaNodeData> _allNodesData = new();
    private List<string> _choices = new();
    private List<ChoiceNodeData> _choicesData = new();

    [ShowIf("HasChoices")] [BoxGroup("Choice Operations")] [SerializeField, ReadOnly]
    private List<string> _choicesText = new();

    private DialoguePm _dialoguePm;

    [ShowIf("HasChoices")] [BoxGroup("Choice Operations")] [ValueDropdown("_choices")]
    public string decision;

    private bool dontHaveEndNode = true;
    private LoggerService _loggerService;

    [ShowIf("HasChoices")]
    [Button]
    [BoxGroup("Choice Operations")]
    [InlineButton("decision")]
    public void MakeChoice()
    {
        // analise dialogue container and all the dialogues in SO
        // get the last option
        var choice = _choicesData.FirstOrDefault(c => c.Choice == decision);
        if (choice != null)
        {
            AllNodesData.Add(choice);
            
            var log = new LogUnit
            {
                system = new List<string>()
                {
                    choice.Guid,
                    choice.GetType().ToString()
                },

                data = new List<string> { choice.Choice, (LocalizedString)choice.Choice },
            };
            allLog.Add(log);

            log = new LogUnit
            {
                system = new List<string>()
                {
                    choice.Guid,
                    choice.GetType().ToString()
                },

                data = new List<string> { choice.Choice, (LocalizedString)choice.Choice },
            };
            choiceLog.Add(log);
        }

        ClearChoices();
        GetData();
    }

    private void ClearChoices()
    {
        _choices = new List<string>();
        _choicesText = new List<string>();
        _choicesData = new List<ChoiceNodeData>();
    }

    private bool HasChoices() => _choices is { Count: > 0 };

    [BoxGroup("Log Operations")]
    [Button]
    public void ClearLog()
    {
        allLog.Clear();
        choiceLog.Clear();
        AllNodesData.Clear();
        dontHaveEndNode = true;
        ClearChoices();
    }

    [ShowIf("dontHaveEndNode")]
    [BoxGroup("Log Operations")]
    [Button]
    public void GetData()
    {
        if (Dialogue == null)
        {
            Debug.LogError($"[{this}] dialogue not set for log object");
            return;
        }

        _loggerService = new LoggerService();
        _loggerService.AddLog(AllNodesData);
        ClearChoices();

        var levelData = new ReactiveProperty<LevelData>(new LevelData(Dialogue.GetNodesData(), Dialogue.NodeLinks));
        var findNext = new ReactiveCommand<List<AaNodeData>>();
        _dialoguePm = new DialoguePm(new DialoguePm.Ctx
        {
            LevelData = levelData,
            DialogueLogger = new DialogueLoggerPm(_loggerService),
            FindNext = findNext
        });

        List<AaNodeData> data;
        List<AaNodeData> newList;
        if (allLog.Count < 1)
        {
            var entryNodeData = levelData.Value.GetEntryNode();
            newList = new List<AaNodeData> { entryNodeData };
        }
        else
        {
            newList = new List<AaNodeData> { new EventNodeData { Guid = allLog[^1].system[0] } };
        }

        data = newList;

        var hasChoices = false;

        while (!hasChoices && dontHaveEndNode)
        {
            var nextCycle = _dialoguePm.FindNext(data);

            data = new();
            _choicesData = new List<ChoiceNodeData>();
            foreach (var next in nextCycle)
            {
                switch (next)
                {
                    case ChoiceNodeData choice:
                    {
                        var isLocked = choice.IsLocked ? " [G] " : string.Empty;
                        hasChoices = true;
                        _choices.Add(choice.Choice);
                        _choicesData.Add(choice);
                        _choicesText.Add($"{choice.Choice}:{isLocked}{(LocalizedString)choice.Choice}");
                        break;
                    }
                    case EndNodeData end:
                        AllNodesData.AddRange(nextCycle);
                        allLog.Add(new LogUnit
                        {
                            system = new List<string>()
                            {
                                next.Guid,
                                next.GetType().ToString()
                            },

                            data = new List<string> { end.End },
                        });
                        
                        dontHaveEndNode = false;
                        break;
                    default:
                    {
                        AllNodesData.AddRange(nextCycle);
                        data.Add(next);

                        var sketchText = next switch
                        {
                            PhraseNodeData phrase => phrase.PhraseSketchText,
                            ImagePhraseNodeData imagePhrase => imagePhrase.PhraseSketchText,
                            _ => string.Empty
                        };

                        allLog.Add(new LogUnit
                        {
                            system = new List<string>()
                            {
                                next.Guid,
                                next.GetType().ToString()
                            },

                            data = new List<string> { sketchText },
                        });
                        break;
                    }
                }
            }
        }

        levelData.Dispose();
        findNext.Dispose();
        _dialoguePm.Dispose();
    }
}


[Serializable, ReadOnly, FoldoutGroup("gr", expanded: false)]
public class LogUnit
{
    public List<string> system;
    [ShowIf("HasData")] public List<string> data;
    [SerializeField] public Dictionary<string, string> dict = new();
    private bool HasData() => data is { Count: > 0 };
}