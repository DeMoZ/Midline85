using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class DialogueGraphView : GraphView
    {
        public DialogueGraphView(AaReactive<LanguageOperation> languageOperation)
        {
            var backgroundStyle = (StyleSheet) EditorGUIUtility.Load("AaDialogueGraph/Styles/GraphViewStyle.uss");
            if (backgroundStyle)
                styleSheets.Add(backgroundStyle);

            var styleSheet = (StyleSheet) EditorGUIUtility.Load("AaDialogueGraph/Styles/PhraseNodeStyle.uss");
            if (styleSheet)
                styleSheets.Add(styleSheet);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();

            languageOperation.Subscribe(OnLanguageChange);
            AddElement(new EntryPointNode(languageOperation));

            CreateMinimap();
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
            var nodeData = new PhraseNodeData();
            var phraseNode = new PhraseNode(nodeData, languages, Guid.NewGuid().ToString());
            phraseNode.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));

            AddElement(phraseNode);
        }

        public void CreateChoiceNode()
        {
            var nodeData = new ChoiceNodeData();
            var choiceNode = new ChoiceNode(nodeData, Guid.NewGuid().ToString());
            choiceNode.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));
            AddElement(choiceNode);
        }

        private Vector2 GetNewNodePosition()
        {
            var worldPosition = Event.current.mousePosition + Vector2.up * 100;
            return contentViewContainer.WorldToLocal(worldPosition);
        }

        private void CreateMinimap()
        {
            var miniMap = new MiniMap {anchored = false};
            miniMap.SetPosition(new Rect(10, 30, 200, 100));
            Add(miniMap);
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
                        phraseContainer?.Add(new PhraseElementsRowField(AaGraphConstants.NewLanguageName,
                            onChange: node.CheckNodeContent));
                        node.CheckNodeContent();
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
                        node.CheckNodeContent();
                    }

                    entryNode.Remove(languageOperation.Element);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}