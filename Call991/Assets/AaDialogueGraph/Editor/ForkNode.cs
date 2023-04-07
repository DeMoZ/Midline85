using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class ForkNode : AaNode
    {
        public void Set(ForkNodeData data, string guid)
        {
            Guid = guid;

            NodeType = AaNodeType.ForkNode;
            title = AaGraphConstants.ForkNode;

            CreateInPort();
            CreateOutPort("Default exit from the node.\nIf none of case exits \"true\"");
            
            var addExitButton = new Button(() =>
            {
                CreateForkExitElement(contentContainer, new ForkCaseData(new CaseData(), System.Guid.NewGuid().ToString()));
            });
            addExitButton.text = "Add case exit";
            contentContainer.Add(addExitButton);
            
            foreach (var forkCase in data.ForkCaseData)
            {
                CreateForkExitElement(contentContainer, new ForkCaseData(forkCase, forkCase.ForkExitName));
            }

            void CreateForkExitElement(VisualElement visualElement, ForkCaseData data)
            {
                var exitContainer = new ForkExitElement();
                exitContainer.Set(this, exitCase => RemoveElement(exitCase, contentContainer),
                    data.Words, data.Ends, data.Counts, data.ForkExitName);
                visualElement.Add(exitContainer);
            }
        }

        private void RemoveElement(VisualElement exitCase, VisualElement container)
        {
            container.Remove(exitCase);
        }

        private class ForkExitElement : VisualElement
        {
            public void Set(AaNode node, Action<VisualElement> onDelete, List<ChoiceData> wordData,
                List<EndData> endData, List<CountData> countData, string exitGuid)
            {
                var deleteButton = new Button(() => { onDelete?.Invoke(this); });
                deleteButton.text = AaGraphConstants.DeleteName;
                contentContainer.Add(deleteButton);

                var caseFoldout = new Foldout { value = false };

                var caseElement = new CaseGroupElement();
                caseElement.Set(caseFoldout, wordData, endData, countData, exitGuid);
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