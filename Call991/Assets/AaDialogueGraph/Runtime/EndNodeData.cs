namespace AaDialogueGraph
{
    [System.Serializable]
    public class EndNodeData : AaNodeData
    { 
        public string End;
        
        public EndNodeData()
        {
            NodeType = AaNodeType.EndNode;
        }
    }
}
