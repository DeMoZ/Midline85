namespace AaDialogueGraph
{
    [System.Serializable]
    public class CountNodeData : AaNodeData
    { 
        public string Choice;
        public int Value = 1;
        
        public CountNodeData()
        {
            NodeType = AaNodeType.CountNode;
        }
    }
}