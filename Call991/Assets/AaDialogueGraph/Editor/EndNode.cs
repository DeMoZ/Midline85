using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class EndNode : AaNode
    {
        public EndNode(EndNodeData data, string guid)
        {
            Guid = guid;
            NodeType = AaNodeType.CountNode;
            titleContainer.Add(new EndPopupField(AaKeys.EndKeys, data.End));

            CreateInPort();

            contentContainer.AddToClassList("aa-EndNode_extension-container");
         }
    }
}