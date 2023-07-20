using System;
using System.Collections.Generic;
using System.Linq;
using AaDialogueGraph;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

public class DialoguePm : IDisposable
{
    public struct Ctx
    {
        public LevelData LevelData;
        public ReactiveCommand<List<AaNodeData>> OnNext;
        public ReactiveCommand<List<AaNodeData>> FindNext;
        public DialogueLoggerPm DialogueLogger;
    }

    private readonly Ctx _ctx;
    private List<AaNodeData> _currentNodes = new();
    private CompositeDisposable _disposables;
    

    public DialoguePm(Ctx ctx)
    {
        _ctx = ctx;
        _disposables = new CompositeDisposable();
        var entryNode = _ctx.LevelData.GetEntryNode();
        
        _ctx.DialogueLogger.Init(entryNode.LevelId);

        _currentNodes.Add(entryNode);
        _ctx.FindNext.Subscribe(OnFindNext).AddTo(_disposables);
    }

    /// <summary>
    /// Look into the dictionary for given nodes, find links for next nodes.
    /// If fork - process to select correct exit.
    /// If non display node, process to select next node.
    /// Process till all collected nodes requires to be displayed.
    /// </summary>
    /// <param name="data"></param>
    private void OnFindNext(List<AaNodeData> data)
    {
        _ctx.DialogueLogger.AddLog(data);
        _currentNodes = FindNext(data.Any() ? data : _currentNodes);

        if (_currentNodes.Any())
            _ctx.OnNext?.Execute(_currentNodes);
        else
            Debug.LogWarning($"[{this}] no next nodes were found");
    }

    private List<AaNodeData> FindNext(List<AaNodeData> datas)
    {
        var result = new List<AaNodeData>();
        foreach (var data in datas)
        {
            if (data == null) continue;

            var nextData = GetNext(data);

            foreach (var next in nextData)
            {
                if (!result.Contains(next)) result.Add(next);
            }
        }

        return result;
    }

    private List<AaNodeData> GetNext(AaNodeData data)
    {
        var links = _ctx.LevelData.Links.Where(l => l.BaseNodeGuid == data.Guid);
        var result = LinksToNodes(links);
        return result;
    }

    private List<AaNodeData> LinksToNodes(IEnumerable<NodeLinkData> links)
    {
        var result = new List<AaNodeData>();

        foreach (var link in links)
        {
            if (_ctx.LevelData.Nodes.TryGetValue(link.TargetNodeGuid, out var aaNodeData))
            {
                if (result.Contains(aaNodeData)) continue;

                switch (aaNodeData)
                {
                    case ChoiceNodeData nodeData:
                    {
                        var isInCase = IsInCase(nodeData.CaseData);

                        if (!isInCase)
                            Debug.LogWarning($"[{this}] button is locked:\n" +
                                             $"{JsonConvert.SerializeObject(nodeData.CaseData)}");

                        nodeData.IsLocked = !isInCase;

                        result.Add(nodeData);
                        break;
                    }
                    case NewspaperNodeData:
                    case EventNodeData:
                    case PhraseNodeData:
                        result.Add(aaNodeData);
                        break;
                    case ForkNodeData nodeData:
                        result.AddRange(GetForkExitNode(nodeData));
                        break;
                    case CountNodeData nodeData:
                        _ctx.DialogueLogger.AddLog(nodeData);
                        _ctx.DialogueLogger.AddCount(nodeData.Choice, nodeData.Value);
                        result.AddRange(GetNext(nodeData));
                        break;
                    case EndNodeData nodeData:
                        _ctx.DialogueLogger.AddLog(nodeData);
                        result.Add(nodeData);
                        break;
                    case EntryNodeData:
                        throw new SystemException("Entry point can not be here. Some issues with graph");
                }
            }
        }

        return result;
    }

    private List<AaNodeData> GetForkExitNode(ForkNodeData data)
    {
        var exit = GetForkExit(data.ForkCaseData);

        if (string.IsNullOrEmpty(exit))
        {
            var exitLinks =
                _ctx.LevelData.Links.Where(l => l.BaseNodeGuid == data.Guid
                                                && string.IsNullOrEmpty(l.BaseExitName));
            return LinksToNodes(exitLinks);
        }
        else
        {
            var exitLinks =
                _ctx.LevelData.Links.Where(l => l.BaseExitName == exit);
            return LinksToNodes(exitLinks);
        }
    }
    
    private string GetForkExit(List<ForkCaseData> data)
    {
        for (var i = data.Count-1; i >= 0; i--)
        {
            if (IsInCase(data[i]))
            {
                Debug.LogWarning($"[{this}] found exit from fork:\n{JsonConvert.SerializeObject(data[i])}");
                return data[i].ForkExitName;
            }
        }

        return null;
    }

    private bool IsInCase(CaseData data)
    {
        var inCase = true;

        foreach (var caseData in data.Words)
        {
            var andWord = false;
            var noWord = false;

            switch (caseData.CaseType)
            {
                case CaseType.AndWord:
                    andWord = _ctx.DialogueLogger.ContainsChoice(caseData.OrKeys);
                    if (andWord) continue;
                    break;
                case CaseType.NoWord:
                    noWord = _ctx.DialogueLogger.ContainsChoice(caseData.OrKeys);
                    if (noWord) continue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!andWord || noWord)
            {
                inCase = false;
                break;
            }
        }

        foreach (var endData in data.Ends)
        {
            var andEnd = false;
            var noEnd = false;

            switch (endData.EndType)
            {
                case EndType.AndEnd:
                    andEnd = _ctx.DialogueLogger.ContainsEnd(endData.OrKeys);
                    if (andEnd) continue;
                    break;
                case EndType.NoEnd:
                    noEnd = _ctx.DialogueLogger.ContainsEnd(endData.OrKeys);
                    if (noEnd) continue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!andEnd || noEnd)
            {
                inCase = false;
                break;
            }
        }

        foreach (var count in data.Counts)
        {
            var value = _ctx.DialogueLogger.GetCount(count.CountKey);
            if (value < count.Range.x || value > count.Range.y)
            {
                inCase = false;
                break;
            }
        }

        return inCase;
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}