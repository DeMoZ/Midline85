using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

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
            AddElement(CreatePhraseNode(nodeName));
        }

        public PhraseNode CreatePhraseNode(string nodeName)
        {
            var node = new PhraseNode
            {
                title = nodeName,
                DialogueText = nodeName,
                Guid = Guid.NewGuid().ToString(),
            };

            var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "input";
            node.inputContainer.Add(inputPort);

            var addChoiceButton = new Button(() => { AddChoicePort(node); });
            addChoiceButton.text = "+";
            node.outputContainer.Add(addChoiceButton);

            var addPhraseAssetButton = new Button(() => { AddLanguageAssetField(node); });
            addPhraseAssetButton.text = "phraseAsset";
            node.contentContainer.Add(addPhraseAssetButton);
            
            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(Vector2.zero, _defaultNodeSize));

            return node;
        }

        // public void Foo()
        // {
        //     var objField_ScriptableObject = new ObjectField
        //     {
        //         objectType = typeof(MyScriptableObject),
        //         allowSceneObjects = false,
        //         value = MyScriptableObject,
        //     };
        //
        //     objField_ScriptableObject.RegisterValueChangedCallback(v =>
        //     {
        //         ScriptableObject = objField_ScriptableObject.value as MyScriptableObject;
        //     });
        //
        //     //mainContainer.Add(objField_ScriptableObject);
        // }

        public void AddChoicePort(PhraseNode node, string overwriteName = null)
        {
            var port = GeneratePort(node, Direction.Output);

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

        private void AddLanguageAssetField(VisualElement node)
        {
            var assetField = new ObjectField
            {
                objectType = typeof(Phrase),
                allowSceneObjects = false,
                //value = MyScriptableObject,
            };
            node.contentContainer.Add(assetField);
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

        private Port GeneratePort(PhraseNode node, Direction portDirection,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }

        private PhraseNode GenerateEntryPointNode()
        {
            var node = new PhraseNode
            {
                title = "Start",
                DialogueText = "Entry Point",
                Guid = Guid.NewGuid().ToString(),
                EntryPoint = true,
            };

            var port = GeneratePort(node, Direction.Output);
            port.portName = "Next";
            node.outputContainer.Add(port);
            
            var addLanguageButton = new Button(() =>
            {
                AddLanguageField(node);
            });
            addLanguageButton.text = "Add Language";
            node.contentContainer.Add(addLanguageButton);

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 200, 150));

            return node;
        }

        private void AddLanguageField(PhraseNode node)
        {
            var language = "new";
            var languageLabel = new Label(language);
            node.Languages.Add(language);
            var languageId = node.Languages.Count - 1;
            
            var languageContainer = new VisualElement();
            
            var textField = new TextField
            {
                name = string.Empty,
                value = language,
            };
            textField.RegisterValueChangedCallback(evt =>
            {
                textField.name = evt.newValue;
                node.Languages[languageId] = evt.newValue;
                languageLabel.text = evt.newValue;
            });

            var deleteLanguageButton = new Button(() => { node.contentContainer.Remove(languageContainer); })
            {
                text = "X",
            };
            
            languageContainer.Add(deleteLanguageButton);
            languageContainer.Add(languageLabel);
            languageContainer.Add(textField);
            
            node.contentContainer.Add(languageContainer);
        }
    }
}