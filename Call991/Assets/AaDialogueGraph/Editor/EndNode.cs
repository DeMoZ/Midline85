namespace AaDialogueGraph.Editor
{
    public class EndNode : AaNode
    {
        public void Set(EndNodeData data, string guid)
        {
            Guid = guid;
            NodeType = AaNodeType.CountNode;
            titleContainer.Add(new EndPopupField(AaKeys.EndKeys, data.End));

            CreateInPort();

            contentContainer.AddToClassList("aa-EndNode_extension-container");
         }
    }
}