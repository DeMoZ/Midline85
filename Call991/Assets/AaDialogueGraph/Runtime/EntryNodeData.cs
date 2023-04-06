using System;
using System.Collections.Generic;
using UnityEngine;

namespace AaDialogueGraph
{
    [Serializable]
    public class EntryNodeData : AaNodeData
    {
        public List<string> Languages = new();
        public string EntryGuid;
        public Rect EntryRect;
    }
}