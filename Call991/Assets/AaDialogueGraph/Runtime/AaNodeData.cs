using UnityEngine;

namespace AaDialogueGraph
{
    public abstract class AaNodeData
    {
        public string Guid;
        public Rect Rect;
        public virtual AaNodeType NodeType { get; protected set; }
    }
}