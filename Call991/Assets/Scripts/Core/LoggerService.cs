using System;
using System.Collections.Generic;
using AaDialogueGraph;
using UnityEngine;

public class LoggerService : IDisposable
{
    public LevelContainer LevelContainer { get; } = new();

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
                AddCaseInCash(nodeData.Choice, LevelContainer.ChoicesCash);
                AddNode(nodeData.Guid, nodeData.Choice);
                break;
            case CountNodeData nodeData:
                AddNode(nodeData.Guid, nodeData.Value.ToString());
                break;
            case EndNodeData nodeData:
                AddCaseInCash(nodeData.End, LevelContainer.EndsCash);
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

    private void AddNode(string key, string value) =>
        LevelContainer.LogCash[key] = value;

    public void Clear()
    {
        LevelContainer.LogCash = new Dictionary<string, string>();
        LevelContainer.ChoicesCash = new Dictionary<string, string>();
        LevelContainer.EndsCash = new Dictionary<string, string>();
        LevelContainer.CountsCash = new Dictionary<string, int>();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}