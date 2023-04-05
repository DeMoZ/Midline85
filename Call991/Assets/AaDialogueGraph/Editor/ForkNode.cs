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
            title = AaGraphConstants.ForkNode;
            
            CreateInPort();
            CreateOutPort("Default exit from the node.\nIf none of case exits \"true\"");
            
            var addExitButton = new Button(() =>
            {
                var exitContainer = new ForkExitElement(this, exitCase => RemoveElement(exitCase, contentContainer),
                    new List<ChoiceData>(), new List<EndData>(), new List<CountData>(), 
                    System.Guid.NewGuid().ToString());
                contentContainer.Add(exitContainer);
            });
            addExitButton.text = "Add case exit";
            contentContainer.Add(addExitButton);

            if (data.CaseData != null)
            {
                foreach (var exit in data.CaseData)
                {
                    var exitContainer = new ForkExitElement(this, exitCase => RemoveElement(exitCase, contentContainer),
                        exit.Words, exit.Ends, exit.Counts, exit.ForkExitName);
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
            public ForkExitElement(AaNode node, Action<VisualElement> onDelete, List<ChoiceData> wordData,
                List<EndData> endData, List<CountData> countData, string exitGuid)
            {
                var deleteButton = new Button(() => { onDelete?.Invoke(this); });
                deleteButton.text = AaGraphConstants.DeleteName;
                contentContainer.Add(deleteButton);

                var caseFoldout = new Foldout { value = false };

                var caseElement = new CaseGroupElement(caseFoldout, wordData, endData, countData, exitGuid);
                caseFoldout.Add(caseElement);
                contentContainer.Add(caseFoldout);

                var caseOutPort = GraphElements.GeneratePort(node, Direction.Output, Port.Capacity.Multi);
                caseOutPort.portName = AaGraphConstants.OutPortName;
                caseOutPort.name = exitGuid;
                contentContainer.Add(caseOutPort);

                contentContainer.style.flexDirection = FlexDirection.Row;
                contentContainer.AddToClassList("aa-ForkNode_extension-container");
            }
        }
    }
}