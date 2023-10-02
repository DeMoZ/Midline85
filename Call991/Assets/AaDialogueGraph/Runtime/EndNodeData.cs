namespace AaDialogueGraph
{
    [System.Serializable]
    public class EndNodeData : AaNodeData
    { 
        public string End;

        public bool SkipSelectNextLevelButtons;
        //public List<RecordData> Records = new();
    }
    
    [System.Serializable]
    public class RecordData
    {
        public string Sprite;
        public string Key;
    }
}
