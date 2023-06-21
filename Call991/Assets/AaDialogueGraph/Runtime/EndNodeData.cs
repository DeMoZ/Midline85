using System.Collections.Generic;

namespace AaDialogueGraph
{
    [System.Serializable]
    public class EndNodeData : AaNodeData
    { 
        public string End;
        public List<EventVisualData> EventVisualData = new();
        public List<RecordData> Records = new();
    }

    [System.Serializable]
    public class RecordData
    {
        public string Sprite;
        public string Key;
    }
}
