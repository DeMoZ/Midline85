using System;
using System.Collections.Generic;

namespace AaDialogueGraph
{
    [Serializable]
    public class EntryNodeData : AaNodeData
    {
        public List<string> Languages = new();
    }
}