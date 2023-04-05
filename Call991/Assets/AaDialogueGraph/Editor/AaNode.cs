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

        protected void CreateInPort(string toolTip = null)
        {
            var port = GraphElements.GeneratePort(this, Direction.Input, Port.Capacity.Multi);
            port.portName = AaGraphConstants.InPortName;
            port.tooltip = toolTip;
            inputContainer.Add(port);
        }

        protected void CreateOutPort(string toolTip = null)
        {
            var port = GraphElements.GeneratePort(this, Direction.Output, Port.Capacity.Multi);
            port.portName = AaGraphConstants.OutPortName;
            port.tooltip = toolTip;
            outputContainer.Add(port);
        }
    }
}