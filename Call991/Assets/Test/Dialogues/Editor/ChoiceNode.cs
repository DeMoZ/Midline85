using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Test.Dialogues
{
    public class ChoiceNode : AaNode
    {
        private static List<string> _choiceKeys = new();

        private static List<string> ChoiceKeys
        {
            get
            {
                if (!_choiceKeys.Any())
                    _choiceKeys = LocalizationManager.GetTermsList().Where(cKey => cKey.Contains("c.word")).ToList();

                return _choiceKeys;
            }
        }

        public ChoiceNode(List<ChoiceCaseData> choices, string guid = null)
        {
            Guid = guid ?? System.Guid.NewGuid().ToString();

            NodeType = AaNodeType.ChoiceNode;
            titleContainer.Add(new ChoicePopupField(ChoiceKeys));

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
                foldout.Add(new AndChoiceCase("and", element => RemoveElement(element, foldout), ChoiceKeys));
                UpdateCasesCount(foldout);
            });
            addAndCase.text = "+And";
            buttonsContainer.Add(addAndCase);

            var addNoCase = new Button(() =>
            {
                foldout.Add(new NoChoiceCase("no", element => RemoveElement(element, foldout), ChoiceKeys));
                UpdateCasesCount(foldout);
            });
            addNoCase.text = "+No";
            buttonsContainer.Add(addNoCase);

            foldout.Add(buttonsContainer);
            contentContainer.Add(foldout);
        }

        private void UpdateCasesCount(Foldout foldout)
        {
            var cnt = foldout.Query<ChoiceCase>().ToList().Count;
            foldout.text = $"Cases {cnt}";
        }

        private void RemoveElement(VisualElement element, Foldout foldout)
        {
            contentContainer.Remove(element);
            UpdateCasesCount(foldout);
        }
    }
}