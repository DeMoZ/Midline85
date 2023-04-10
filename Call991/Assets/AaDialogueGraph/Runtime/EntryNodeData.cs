using System;
using System.Collections.Generic;

namespace AaDialogueGraph
{
    [Serializable]
    public class EntryNodeData : AaNodeData
    {
        public List<string> Languages = new();

        public override AaNodeType NodeType { get; protected set; } = AaNodeType.EntryNode;
        //
        // public EntryNodeData()
        // {
        //     NodeType = AaNodeType.EntryNode;
        // }
    }
}