using System;
using System.Collections.Generic;
using System.Linq;
using AaDialogueGraph;
using I2.Loc;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

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

    // [Button]
    // [HorizontalGroup("PrefsControl")]
    // public void LoadFromPrefs()
    // {
    // }

    [Button]
    [HorizontalGroup("PrefsControl")]
    public void SaveToPrefs()
    {
        //PlayerPrefs.SetString(AaConstants.GameProgress, data);
    }
}

[Serializable]
public class DialogueLog
{
    public DialogueContainer dialogue;
    [SerializeField] private bool showOnlyButtons = true;
    private bool ShowAllLogs => !showOnlyButtons;

    [ReadOnly, ShowIf("ShowAllLogs")] public List<LogUnit> allLog = new();
    [ReadOnly, ShowIf("showOnlyButtons")] public List<LogUnit> choiceLog = new();

    private List<string> _choices = new();
    private List<ChoiceNodeData> _choicesData = new();

    [ShowIf("HasChoices")] [BoxGroup("Choice Operations")] [SerializeField, ReadOnly]
    private List<string> _choicesText = new();

    private DialoguePm _dialoguePm;

    [ShowIf("HasChoices")] [BoxGroup("Choice Operations")] [ValueDropdown("_choices")]
    public string decision;

    private bool dontHaveEndNode = true;
    
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
        allLog = new List<LogUnit>();
        choiceLog = new List<LogUnit>();
        dontHaveEndNode = true;
        ClearChoices();
    }

    [ShowIf("dontHaveEndNode")]
    [BoxGroup("Log Operations")]
    [Button]
    public void GetData()
    {
        if (dialogue == null)
        {
            Debug.LogError($"[{this}] dialogue not set for log object");
            return;
        }

        ClearChoices();

        var levelData = new ReactiveProperty<LevelData>(new LevelData(dialogue.GetNodesData(), dialogue.NodeLinks));
        var findNext = new ReactiveCommand<List<AaNodeData>>();
        _dialoguePm = new DialoguePm(new DialoguePm.Ctx
            { LevelData = levelData, DialogueLogger = new DialogueLoggerPm(), FindNext = findNext });

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
            var cycleNexts = _dialoguePm.FindNext(data);
            data = new();
            _choicesData = new List<ChoiceNodeData>();
            foreach (var next in cycleNexts)
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

    [SerializeField] public Dictionary<string, string> dict = new Dictionary<string, string>()
        { { "a1", "b1" }, { "a2", "b2" }, };

    private bool HasData() => data is { Count: > 0 };
}

public enum Decision
{
}