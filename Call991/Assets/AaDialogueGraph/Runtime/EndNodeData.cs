using System.Collections.Generic;

namespace AaDialogueGraph
{
    [System.Serializable]
    public class EndNodeData : AaNodeData
    { 
        public string End;
        public List<EventVisualData> EventVisualData;
        public List<RecordData> Records;
    }

    [System.Serializable]
    public class RecordData
    {
        public string Sprite;
        public string Key;
    }
}
