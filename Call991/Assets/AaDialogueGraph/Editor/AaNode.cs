using UnityEditor.Experimental.GraphView;

namespace AaDialogueGraph.Editor
{
    public abstract class AaNode : Node
    {
        public string Guid;
        public AaNodeType NodeType;
        
        public bool EntryPoint => NodeType == AaNodeType.EntryPoint;
    }
}