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
    }

    private readonly Ctx _ctx;
    private List<AaNodeData> _currentNodes = new();
    private CompositeDisposable _disposables;
    private DialogueLoggerPm _dialogueLoggerPm;

    public DialoguePm(Ctx ctx)
    {
        _ctx = ctx;
        _disposables = new CompositeDisposable();
        _dialogueLoggerPm = new DialogueLoggerPm().AddTo(_disposables);

        _currentNodes.Add(_ctx.LevelData.GetEntryNode());
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
        // todo where all the writings into prefs should be done??
        // here?
        // how to show locked choce nodes?

        _dialogueLoggerPm.AddLog(data);

        _currentNodes = FindNext(data.Any() ? data : _currentNodes);

        if (_currentNodes.Any())
        {
            _ctx.OnNext?.Execute(_currentNodes);
        }
        else
        {
            Debug.LogWarning($"[{this}] no next nodes were found");
        }
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
        var result = new List<AaNodeData>();
        var links = _ctx.LevelData.Links.Where(l => l.BaseNodeGuid == data.Guid);
        result = LinksToNodes(links);
        return result;
    }

    private List<AaNodeData> LinksToNodes(IEnumerable<NodeLinkData> links)
    {
        var result = new List<AaNodeData>();

        foreach (var link in links)
        {
            if (_ctx.LevelData.Nodes.TryGetValue(link.TargetNodeGuid, out var nodeData))
            {
                if (result.Contains(nodeData)) continue;

                switch (nodeData)
                {
                    case ChoiceNodeData choiceData:
                    {
                        var isInCase = IsInCase(choiceData.CaseData);
                        
                        if (!isInCase) Debug.LogWarning($"[{this}] button is locked:\n" +
                                                        $"{JsonConvert.SerializeObject(choiceData.CaseData)}");

                        choiceData.IsLocked = !isInCase;

                        result.Add(choiceData);
                        break;
                    }
                    case PhraseNodeData:
                        result.Add(nodeData);
                        break;
                    case ForkNodeData forkData:
                        var exits = GetForkExit(forkData.ForkCaseData);

                        if (!exits.Any())
                        {
                            var exitLinks = 
                                _ctx.LevelData.Links.Where(l => l.BaseNodeGuid == forkData.Guid 
                                                                            && string.IsNullOrEmpty(l.BaseExitName));
                            result.AddRange(LinksToNodes(exitLinks));
                        }
                        else
                        {
                            foreach (var exit in exits)
                            {
                                var exitLinks = 
                                    _ctx.LevelData.Links.Where(l => l.BaseExitName == exit);
                                result.AddRange(LinksToNodes(exitLinks));
                            }
                        }

                        break;
                    case CountNodeData countData:
                        _dialogueLoggerPm.AddLog(countData);
                        _dialogueLoggerPm.AddCount(countData.Choice, countData.Value);
                        result.AddRange(GetNext(countData));
                        break;
                    case EndNodeData:

                        break;
                    case EntryNodeData:
                        throw new SystemException("Entry point can not be here. Some issues with graph");
                }
            }
        }

        return result;
    }

    private List<string> GetForkExit(List<ForkCaseData> data)
    {
        var result = new List<string>();
        
        foreach (var condition in data)
        {
            var isInCase = IsInCase(condition);
            if (isInCase)
            {
                Debug.LogWarning($"[{this}] found exit from fork:\n{JsonConvert.SerializeObject(condition)}");
                result.Add(condition.ForkExitName);
                break;
            }
        }

        return result;
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
                    andWord = _dialogueLoggerPm.ContainsChoice(caseData.OrKeys);
                    if (andWord) continue;
                    break;
                case CaseType.NoWord:
                    noWord = _dialogueLoggerPm.ContainsChoice(caseData.OrKeys);
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
                    andEnd = _dialogueLoggerPm.ContainsEnd(endData.OrKeys);
                    if (andEnd) continue;
                    break;
                case EndType.NoEnd:
                    noEnd = _dialogueLoggerPm.ContainsEnd(endData.OrKeys);
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
            var value = _dialogueLoggerPm.GetCount(count.CountKey);
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