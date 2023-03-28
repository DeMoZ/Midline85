using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class ForkNode : AaNode
    {
        public ForkNode(ForkNodeData data, string guid)
        {
            Guid = guid;

            NodeType = AaNodeType.ForkNode;
            title = AaGraphConstants.NewForkNode;

            var inPort = GraphElements.GeneratePort(this, Direction.Input, Port.Capacity.Multi);
            inPort.portName = AaGraphConstants.InPortName;
            inputContainer.Add(inPort);

            var outPort = GraphElements.GeneratePort(this, Direction.Output, Port.Capacity.Multi);
            outPort.portName = AaGraphConstants.OutPortName;
            outPort.tooltip = "Default exit from the node.\nIf none of case exits \"true\"";
            outputContainer.Add(outPort);

            var addExitButton = new Button(() =>
            {
                var exitContainer = new ForkExitElement(this, exitCase => RemoveElement(exitCase, contentContainer),
                    new List<CaseData>());
                contentContainer.Add(exitContainer);
            });
            addExitButton.text = "Add case exit";
            contentContainer.Add(addExitButton);

            if (data.Exits != null)
            {
                foreach (var exit in data.Exits)
                {
                    var exitContainer = new ForkExitElement(this, exitCase => RemoveElement(exitCase, contentContainer),
                        exit.Cases);
                    contentContainer.Add(exitContainer);
                }
            }
        }

        private void RemoveElement(VisualElement exitCase, VisualElement container)
        {
            container.Remove(exitCase);
        }

        public class ForkExitElement : VisualElement
        {
            public ForkExitElement(AaNode node, Action<VisualElement> onDelete, List<CaseData> data)
            {
                var deleteButton = new Button(() => { onDelete?.Invoke(this); });
                deleteButton.text = AaGraphConstants.DeleteName;
                contentContainer.Add(deleteButton);

                var caseFoldout = new Foldout { value = false };

                var caseElement = new CaseGroupElement(caseFoldout, data);
                caseFoldout.Add(caseElement);
                contentContainer.Add(caseFoldout);

                var caseOutPort = GraphElements.GeneratePort(node, Direction.Output, Port.Capacity.Multi);
                caseOutPort.portName = AaGraphConstants.OutPortName;
                contentContainer.Add(caseOutPort);

                contentContainer.style.flexDirection = FlexDirection.Row;
                contentContainer.AddToClassList("aa-ForkNode_extension-container");
            }
        }
    }
}