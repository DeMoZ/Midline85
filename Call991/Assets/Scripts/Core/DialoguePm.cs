using System;
using System.Collections.Generic;
using System.Linq;
using AaDialogueGraph;
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
        _dialogueLoggerPm = new DialogueLoggerPm();
        
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

        foreach (var link in links)
        {
            if (_ctx.LevelData.Nodes.TryGetValue(link.TargetNodeGuid, out var nodeData))
            {
                if (result.Contains(nodeData)) continue;

                switch (nodeData)
                {
                    case ChoiceNodeData choiceData:
                    {
                        // calculate if locked
                        if (!choiceData.IsCaseResolved())
                        {
                            choiceData.IsLocked = true;
                        }

                        result.Add(choiceData);
                        break;
                    }
                    case PhraseNodeData:
                        result.Add(nodeData);
                        break;
                    case ForkNodeData forkData:

                        var exit = GetForkExit(forkData.ForkCaseData);
                        result.Add(exit);
                        
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
                        break;
                }
            }
        }

        return result;
    }

    private AaNodeData GetForkExit(List<ForkCaseData> data)
    {
       // fork has default exit
       // fork has case exits
       // need to pass throug cases and found if i can select one.
       // if no - select default exit.
        
      // the method should has one more layers for determinig what type on nodes is next 
      return null;
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}