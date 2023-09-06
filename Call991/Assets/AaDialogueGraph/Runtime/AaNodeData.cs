using System.Collections.Generic;
using UnityEngine;

namespace AaDialogueGraph
{
    public abstract class AaNodeData
    {
        public string Guid;
        public Rect Rect;
        public List<EventVisualData> EventVisualData;
    }
}