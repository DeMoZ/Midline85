using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public abstract class AaNode : Node
    {
        public string Guid;
        protected AaNodeType NodeType;
        
        public bool EntryPoint => NodeType == AaNodeType.EntryNode;

        public AaNode()
        {
            titleContainer.Remove(titleButtonContainer);
        }

        public virtual List<VisualEvent> GetEventsVisual()
        {
            var musics = contentContainer.Query<MusicEventVisual>().ToList();
            var sounds = contentContainer.Query<SoundEventVisual>().ToList();
            var objects = contentContainer.Query<ObjectEventVisual>().ToList();
            var events = new List<VisualEvent>();
            events.AddRange(musics);
            events.AddRange(sounds);
            events.AddRange(objects);

            return events;
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