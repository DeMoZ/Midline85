using System;
using System.Collections.Generic;
using System.Linq;
using AaDialogueGraph;
using UniRx;

public class DialoguePm : IDisposable 
{
    public struct Ctx
    {
        public LevelData LevelData;
        public ReactiveCommand<List<AaNodeData>> OnNext;
        public ReactiveCommand<List<AaNodeData>> FindNext;
    }
 
    private readonly Ctx _ctx;
    private List<AaNodeData> _currentNodes = new ();
    private CompositeDisposable _disposables;
    
    public DialoguePm(Ctx ctx)
    {
        _ctx = ctx;
        _disposables = new CompositeDisposable();
        
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

        _currentNodes = FindNext(data.Any() ? data : _currentNodes);
        _ctx.OnNext?.Execute(_currentNodes);
    }

    private List<AaNodeData> FindNext(List<AaNodeData> datas)
    {
        var result = new List<AaNodeData>();
        foreach (var data in datas)
        {
            var nextData = GetNext(data);
            if (nextData != null)
            {
                result.AddRange(nextData);
            }
        }

        return result;
    }

    private List<AaNodeData> GetNext(AaNodeData data)
    {
        // first - need to calculate all points, from countNodes
        // put in logs all choices keys
        // put in logs all phrases
        
        // second - find all exit nodes
        // calculate fork exit nodes and find the right exit
        
        // third - find all nodes by exits
        // populate results
        
        var result = new List<AaNodeData>();
        var targetNodes = new List<AaNodeData>(); //will make calculations

        //var guid = data.Guid;
        var links = _ctx.LevelData.Links.Where(l => l.BaseNodeGuid == data.Guid);

        foreach (var link in links)
        {
            if(_ctx.LevelData.Nodes.TryGetValue(link.TargetNodeGuid, out var nodeData))
            {
                switch (nodeData)
                {
                    case ChoiceNodeData:
                        // calculate if locked
                        var choiceData = (ChoiceNodeData)nodeData;
                        if (!choiceData.IsCaseResolved())
                        {
                            choiceData.IsLocked = true;
                        }

                        result.Add(choiceData);
                        break;
                    case PhraseNodeData:
                        result.Add(nodeData);
                        break;
                    case ForkNodeData:
                
                        break;
                    case CountNodeData:

                        break;
                    case EndNodeData:

                        break;
                    case EntryNodeData:
                        throw new SystemException("Entry point can not be here. Some issues with graph");
                        break;
                }
                
                targetNodes.Add(nodeData);
            }
        }
        
        result.AddRange(targetNodes);
        return result;
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
