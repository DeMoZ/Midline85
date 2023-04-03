using UnityEditor.Experimental.GraphView;

namespace AaDialogueGraph.Editor
{
    public abstract class AaNode : Node
    {
        public string Guid;
        protected AaNodeType NodeType;
        
        public bool EntryPoint => NodeType == AaNodeType.EntryPoint;

        public AaNode()
        {
            titleContainer.Remove(titleButtonContainer);
        }
    }
}