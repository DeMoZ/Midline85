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

        public ChoiceNode(List<ChoiceCaseData> choices = null)
        {
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
                foldout.Add(new AndChoiceCase("and", element => RemoveElement(element, foldout)));
                UpdateCasesCount(foldout);
            });
            addAndCase.text = "+And";
            buttonsContainer.Add(addAndCase);

            var addNoCase = new Button(() =>
            {
                foldout.Add(new NoChoiceCase("no", element => RemoveElement(element, foldout)));
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

        public abstract class ChoiceCase : VisualElement
        {
            public ChoiceCase(string caseName, Action<ChoiceCase> onDelete)
            {
                contentContainer.style.flexDirection = FlexDirection.Row;

                contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
                {
                    text = "x",
                });
                contentContainer.Add(new Label(caseName));

                contentContainer.Add(new ChoicePopupField(ChoiceKeys));

                contentContainer.Add(new Button(AddCaseField)
                {
                    text = "or",
                });
            }

            private void AddCaseField() =>
                contentContainer.Add(new ChoicePopupField(ChoiceKeys));
        }

        public class ChoicePopupField : VisualElement
        {
            public string Value { get; private set; }

            public ChoicePopupField(List<string> keys)
            {
                var label = new Label();

                contentContainer.Add(new NoEnumPopup(ChoiceKeys, val => label.text = KeyToTextTitle(val)));
                contentContainer.Add(label);
            }

            private string KeyToTextTitle(string val)
            {
                Value = val;
                string textValue = new LocalizedString(val);
                textValue = textValue.Split(" ")[0];
                return textValue;
            }
        }

        /// <summary>
        /// To easily find data for save/load
        /// </summary>
        public class AndChoiceCase : ChoiceCase
        {
            public AndChoiceCase(string caseName, Action<ChoiceCase> onDelete) : base(caseName, onDelete)
            {
            }
        }

        /// <summary>
        /// To easily find data for save/load
        /// </summary>
        public class NoChoiceCase : ChoiceCase
        {
            public NoChoiceCase(string caseName, Action<ChoiceCase> onDelete) : base(caseName, onDelete)
            {
            }
        }
    }
}