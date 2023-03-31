using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class ChoiceNode : AaNode
    {
        public ChoiceNode(ChoiceNodeData data, string guid)
        {
            Guid = guid;

            NodeType = AaNodeType.ChoiceNode;
            titleContainer.Add(new ChoicePopupField(AaChoices.ChoiceKeys, data.Choice));

            var inPort = GraphElements.GeneratePort(this, Direction.Input, Port.Capacity.Multi);
            inPort.portName = AaGraphConstants.InPortName;
            inputContainer.Add(inPort);

            var outPort = GraphElements.GeneratePort(this, Direction.Output, Port.Capacity.Multi);
            outPort.portName = AaGraphConstants.OutPortName;
            outputContainer.Add(outPort);

            var caseFoldout = new Foldout();
            caseFoldout.value = false;
            
            var caseElement = new CaseGroupElement(caseFoldout, data.Words);
            caseFoldout.Add(caseElement);
            contentContainer.Add(caseFoldout);
            caseFoldout.AddToClassList("aa-ChoiceNode_extension-container");
        }
    }
}