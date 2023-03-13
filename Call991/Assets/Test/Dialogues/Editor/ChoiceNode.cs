using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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

        public ChoiceNode(List<ChoiceData> choices = null)
        {
            NodeType = AaNodeType.ChoiceNode;
            title = "Choice";

            var keysPopup = new PopupField<string>("", ChoiceKeys, ChoiceKeys?[0] ?? AaGraphConstants.NoData,
                KeyToTextTitle());
            titleContainer.Add(keysPopup);

            var inPort = GraphElements.GeneratePort(this, Direction.Input, Port.Capacity.Multi);
            inPort.portName = AaGraphConstants.InPortName;
            inputContainer.Add(inPort);

            var outPort = GraphElements.GeneratePort(this, Direction.Output, Port.Capacity.Multi);
            outPort.portName = AaGraphConstants.OutPortName;
            outputContainer.Add(outPort);

            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Row;

            buttonsContainer.Add(new Label("Case"));
            
            var addAndCase = new Button(() => { contentContainer.Add(new AndChoiceCase("and", RemoveElement)); });
            addAndCase.text = "+And";
            buttonsContainer.Add(addAndCase);

            var addNoCase = new Button(() => { contentContainer.Add(new NoChoiceCase("no", RemoveElement)); });
            addNoCase.text = "+No";
            buttonsContainer.Add(addNoCase);

            contentContainer.Add(buttonsContainer);
        }

        private void RemoveElement(VisualElement element)
        {
            contentContainer.Remove(element);
        }

        private Func<string, string> KeyToTextTitle()
        {
            return val =>
            {
                string textValue = new LocalizedString(val);
                textValue = textValue.Split(" ")[0];
                title = textValue;
                return val;
            };
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

                var keysPopup = new PopupField<string>("", ChoiceKeys, ChoiceKeys?[0] ?? AaGraphConstants.NoData);
                contentContainer.Add(keysPopup);

                contentContainer.Add(new Button(() => { })
                {
                    text = "or",
                });
            }
        }

        public class AndChoiceCase : ChoiceCase
        {
            public AndChoiceCase(string caseName, Action<ChoiceCase> onDelete) : base(caseName, onDelete)
            {
            }
        }

        public class NoChoiceCase : ChoiceCase
        {
            public NoChoiceCase(string caseName, Action<ChoiceCase> onDelete) : base(caseName, onDelete)
            {
            }
        }
    }
}