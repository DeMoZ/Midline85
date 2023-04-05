namespace AaDialogueGraph.Editor
{
    public class CountNode : AaNode
    {
        public CountNode(CountNodeData data, string guid)
        {
            Guid = guid;

            NodeType = AaNodeType.CountNode;
            title = AaGraphConstants.CountNode;
            
            CreateInPort();
            CreateOutPort();
        }
    }
}