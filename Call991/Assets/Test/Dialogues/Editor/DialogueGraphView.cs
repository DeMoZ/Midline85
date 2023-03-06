using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Test.Dialogues
{
    public class DialogueGraphView : GraphView
    {
        private readonly Vector2 _defaultNodeSize = new(150, 200);

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

            AddElement(new EntryPointNode());
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
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

        public void CreatePhraseNode(string nodeName)
        {
            var languages = contentContainer.Query<EntryPointNode>().First().GetLanguages() ?? new List<string>();
            var nodeData = new PhraseNodeData {DialogueText = "Phrase"};
            AddElement(CreatePhraseNode(nodeData, languages));
        }
        
        public void CreateChoiceNode(string nodeName)
        {
            //AddElement(CreateChoiceNode(nodeName));
        }

        public PhraseNode CreatePhraseNode(PhraseNodeData nodeData, List<string> languages)
        {
            var node = new PhraseNode
            {
                title = nodeData.DialogueText,
                DialogueText = nodeData.DialogueText,
                Guid = Guid.NewGuid().ToString(),
            };

            var line0 = new Label("   Person");
            node.contentContainer.Add(line0);
            
            var personVisual = new PersonVisual(nodeData.PersonVisualData);
            node.contentContainer.Add(personVisual);

            var line1 = new Label("   Phrase");
            node.contentContainer.Add(line1);

            var phraseVisual = new PhraseVisual(nodeData.PhraseVisualData);
            node.contentContainer.Add(phraseVisual);
            
            var line2 = new Label(" ");
            node.contentContainer.Add(line2);

            var phraseEvents = new PhraseEvents(nodeData.EventVisualData);
            node.contentContainer.Add(phraseEvents);
            
            var line3 = new Label(" ");
            node.contentContainer.Add(line3);

            var addPhraseAssetButton = new Button(() => { node.contentContainer.Add(new PhraseAssetField()); });
            addPhraseAssetButton.text = "Phrase Asset";
            node.contentContainer.Add(addPhraseAssetButton);

            for (var i = 0; i < languages.Count; i++)
            {
                var phrase = nodeData.Phrases.Count > i ? nodeData.Phrases[i] : null;
                node.contentContainer.Add(new PhraseAssetField(phrase));
            }

            var inputPort = GraphElements.GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "input";
            node.inputContainer.Add(inputPort);

            var addChoiceButton = new Button(() => { AddChoicePort(node); });
            addChoiceButton.text = "+";
            node.outputContainer.Add(addChoiceButton);
            
            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(Vector2.zero, _defaultNodeSize));

            return node;
        }

        public void AddChoicePort(PhraseNode node, string overwriteName = null)
        {
            var port = GraphElements.GeneratePort(node, Direction.Output);

            var oldLabel = port.contentContainer.Q<Label>("type");
            port.contentContainer.Remove(oldLabel);

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

            var deletePortButton = new Button(() => RemovePort(node, port))
            {
                text = "X",
            };
            port.contentContainer.Add(deletePortButton);

            node.outputContainer.Add(port);
            node.RefreshExpandedState();
            node.RefreshPorts();
        }

        private void RemovePort(PhraseNode node, Port port)
        {
            var targetAges =
                edges.ToList().Where(x => x.output.portName == port.portName && x.output.node == port.node);

            if (!targetAges.Any()) return;

            var edge = targetAges.First();
            edge.input.Disconnect(edge);

            RemoveElement(targetAges.First());
            node.outputContainer.Remove(port);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }
    }
}