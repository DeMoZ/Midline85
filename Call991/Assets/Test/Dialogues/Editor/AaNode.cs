using UnityEditor.Experimental.GraphView;

namespace Test.Dialogues
{
    public abstract class AaNode : Node
    {
        public string Guid;
        public AaNodeType NodeType;
        
        public bool EntryPoint => NodeType == AaNodeType.EntryPoint;
    }
}