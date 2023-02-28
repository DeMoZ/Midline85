using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Dialogues
{
    public class DialogueGraphView : GraphView
    {
        private readonly Vector2 _defaultNodeSize = new Vector2(150, 200);

        public DialogueGraphView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphBackground"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();

            AddElement(GenerateEntryPointNode());
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            //return base.GetCompatiblePorts(startPort, nodeAdapter);

            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        public void CreateNode(string nodeName)
        {
            AddElement(CreateDialogueNode(nodeName));
        }

        public DialogueNode CreateDialogueNode(string nodeName)
        {
            var node = new DialogueNode
            {
                title = nodeName,
                DialogueText = nodeName,
                Guid = Guid.NewGuid().ToString(),
            };

            var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "input";
            node.inputContainer.Add(inputPort);

            var button = new Button(() => { AddChoicePort(node); });
            button.text = "+";
            node.outputContainer.Add(button);

            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(Vector2.zero, _defaultNodeSize));

            return node;
        }

        public void AddChoicePort(DialogueNode node, string overwriteName = null)
        {
            var port = GeneratePort(node, Direction.Output);
            var outputPortCount = node.outputContainer.Query("connector").ToList().Count;

            var choicePortName = string.IsNullOrEmpty(overwriteName) ? $"Choice {outputPortCount + 1}" : overwriteName;
            port.portName = choicePortName;

            var textField = new TextField
            {
                name = String.Empty,
                value = choicePortName,
            };
            textField.RegisterValueChangedCallback(evt => port.portName = evt.newValue);
            port.contentContainer.Add(new Label("  "));
            port.Add(textField);

            var deleteButton = new Button(() => RemoveElement(port))
            {
                text = "X",
            };
            port.contentContainer.Add(deleteButton);
            
            node.outputContainer.Add(port);
            node.RefreshExpandedState();
            node.RefreshPorts();
        }

        private Port GeneratePort(DialogueNode node, Direction portDirection,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }

        private DialogueNode GenerateEntryPointNode()
        {
            var node = new DialogueNode
            {
                title = "Start",
                DialogueText = "Entry Point",
                Guid = Guid.NewGuid().ToString(),
                EntryPoint = true,
            };

            var port = GeneratePort(node, Direction.Output);
            port.portName = "Next";
            node.outputContainer.Add(port);
            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));

            return node;
        }
    }
}