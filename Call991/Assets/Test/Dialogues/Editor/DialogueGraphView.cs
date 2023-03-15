using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Dialogues
{
    public class DialogueGraphView : GraphView
    {
        public DialogueGraphView(AaReactive<LanguageOperation> languageOperation)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphBackground"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();

            languageOperation.Subscribe(OnLanguageChange);
            AddElement(new EntryPointNode(languageOperation));
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

        public void CreatePhraseNode()
        {
            var languages = contentContainer.Query<EntryPointNode>().First().GetLanguages() ?? new List<string>();
            var nodeData = new PhraseNodeData ();
            AddElement(new PhraseNode(nodeData, languages));
        }

        public void CreateChoiceNode(string nodeName)
        {
            AddElement(new ChoiceNode());
        }

        /// <summary>
        /// On Entry Node add/change/remove language process the operations
        /// </summary>
        private void OnLanguageChange(LanguageOperation languageOperation)
        {
            var phraseNodes = contentContainer.Query<PhraseNode>().ToList();

            var entryNode = contentContainer.Q<EntryPointNode>();
            var languageFields = entryNode.Query<LanguageField>().ToList();
            var index = languageFields.IndexOf(languageOperation.Element as LanguageField);

            switch (languageOperation.Type)
            {
                case LanguageOperationType.Add:
                    foreach (var node in phraseNodes)
                    {
                        if (node.EntryPoint) continue;

                        var phraseContainer = node.Q<PhraseElementsTable>();
                        phraseContainer?.Add(new PhraseElementsRowField(AaGraphConstants.NewLanguageName));
                    }

                    break;
                case LanguageOperationType.Change:
                    foreach (var node in phraseNodes)
                    {
                        if (node.EntryPoint) continue;

                        var rows = node.Query<PhraseElementsRowField>().ToList();
                        var row = rows[index];
                        var label = row.Query<Label>().First();
                        label.text = languageOperation.Value;
                    }

                    break;

                case LanguageOperationType.Remove:
                    foreach (var node in phraseNodes)
                    {
                        if (node.EntryPoint) continue;

                        var rows = node.Query<PhraseElementsRowField>().ToList();
                        var row = rows[index];
                        var phraseContainer = node.Q<PhraseElementsTable>();
                        phraseContainer.Remove(row);
                    }

                    entryNode.Remove(languageOperation.Element);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}