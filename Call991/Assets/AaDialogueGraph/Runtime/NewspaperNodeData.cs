using System;
using System.Collections.Generic;

namespace AaDialogueGraph
{
    [Serializable]
    public class NewspaperNodeData : AaNodeData
    {
        public List<EventVisualData> EventVisualData;
        public List<string> Sprites;
    }
}