using System.Collections.Generic;
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

            var foldout = new Foldout();
            foldout.value = false;
            UpdateCasesCount(foldout);

            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Row;

            var addAndCase = new Button(() =>
            {
                foldout.Add(new AndChoiceCase("and", element => RemoveElement(element, foldout), AaChoices.ChoiceKeys));
                UpdateCasesCount(foldout);
            });
            addAndCase.text = AaGraphConstants.AndWord;
            buttonsContainer.Add(addAndCase);

            var addNoCase = new Button(() =>
            {
                foldout.Add(new NoChoiceCase("no", element => RemoveElement(element, foldout), AaChoices.ChoiceKeys));
                UpdateCasesCount(foldout);
            });
            addNoCase.text = AaGraphConstants.NoWord;
            buttonsContainer.Add(addNoCase);

            foldout.Add(buttonsContainer);

            CreateCases(foldout, data.Cases);

            UpdateCasesCount(foldout);

            contentContainer.Add(foldout);
            foldout.AddToClassList("aa-ChoiceNode_extension-container");
        }

        private void CreateCases(Foldout foldout, List<CaseData> data)
        {
            if (data == null || data.Count <1 ) return;
                
            foreach (var caseData in data)
            {
                if (caseData?.Cases == null || caseData.Cases.Count < 1) continue;

                ChoiceCase aCase;
                if (caseData.And)
                {
                    aCase = new AndChoiceCase("and", element =>
                        RemoveElement(element, foldout), AaChoices.ChoiceKeys, caseData.Cases[0]);
                }
                else
                {
                    aCase = new NoChoiceCase("no", element =>
                        RemoveElement(element, foldout), AaChoices.ChoiceKeys, caseData.Cases[0]);
                }

                for (var i = 1; i < caseData.Cases.Count; i++)
                {
                    aCase.AddCaseField(caseData.Cases[i]);
                }

                foldout.Add(aCase);
            }
        }

        private void UpdateCasesCount(Foldout foldout)
        {
            var cnt = foldout.Query<ChoiceCase>().ToList().Count;
            foldout.text = $"Cases {cnt}";
        }

        private void RemoveElement(VisualElement element, Foldout foldout)
        {
            foldout.Remove(element);
            UpdateCasesCount(foldout);
        }
    }
}