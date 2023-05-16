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
            var backgroundStyle = (StyleSheet)EditorGUIUtility.Load("AaDialogueGraph/Styles/GraphViewStyle.uss");
            if (backgroundStyle)
                styleSheets.Add(backgroundStyle);

            var styleSheet = (StyleSheet)EditorGUIUtility.Load("AaDialogueGraph/Styles/PhraseNodeStyle.uss");
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
            var node = new EntryNode();
            node.Set(languageOperation);
            AddElement(node);

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
            var languages = contentContainer.Q<EntryNode>().GetLanguages() ?? new List<string>();
            var node = new PhraseNode();
            node.Set(new PhraseNodeData(), languages, Guid.NewGuid().ToString());
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));

            AddElement(node);
        }

        public void CreateChoiceNode()
        {
            var node = new ChoiceNode();
            node.Set(new ChoiceNodeData(), Guid.NewGuid().ToString());
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));
            AddElement(node);
        }

        public void CreateForkNode()
        {
            var node = new ForkNode();
            node.Set(new ForkNodeData(), Guid.NewGuid().ToString());
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));
            AddElement(node);
        }

        public void CreateCountNode()
        {
            var node = new CountNode();
            node.Set(new CountNodeData(), Guid.NewGuid().ToString());
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));
            AddElement(node);
        }

        public void CreateEventNode()
        {
            var node = new EventNode();
            node.Set(new EventNodeData(), Guid.NewGuid().ToString());
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));
            AddElement(node);
        }

        public void CreateEndNode()
        {
            var node = new EndNode();
            node.Set(new EndNodeData(), Guid.NewGuid().ToString());
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));
            AddElement(node);
        }

        public void CreateNewspaperNode()
        {
            var languages = contentContainer.Q<EntryNode>().GetLanguages() ?? new List<string>();
            var node = new NewspaperNode();
            node.Set(new NewspaperNodeData(), languages, Guid.NewGuid().ToString());
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));

            AddElement(node);
        }


        private Vector2 GetNewNodePosition()
        {
            var worldPosition = Event.current.mousePosition + Vector2.up * 100;
            return contentViewContainer.WorldToLocal(worldPosition);
        }

        private void CreateMinimap()
        {
            var miniMap = new MiniMap { anchored = false };
            miniMap.SetPosition(new Rect(10, 30, 200, 100));
            miniMap.maxHeight = 100;
            Add(miniMap);
        }

        /// <summary>
        /// On Entry Node add/change/remove language process the operations
        /// </summary>
        private void OnLanguageChange(LanguageOperation languageOperation)
        {
            var phraseNodes = contentContainer.Query<PhraseNode>().ToList();
            var newspaperNodes = contentContainer.Query<NewspaperNode>().ToList();

            var entryNode = contentContainer.Q<EntryNode>();
            var languageFields = entryNode.Query<LanguageField>().ToList();
            var index = languageFields.IndexOf(languageOperation.Element as LanguageField);

            switch (languageOperation.Type)
            {
                case LanguageOperationType.Add:
                    foreach (var node in phraseNodes)
                    {
                        var tableContainer = node.Q<ElementsTable>();
                        var field = new PhraseElementsRowField();
                        field.Set(AaKeys.LanguageKeys[0], onChange: node.CheckNodeContent);
                        tableContainer?.Add(field);
                        node.CheckNodeContent();
                    }

                    foreach (var node in newspaperNodes)
                    {
                        var tableContainer = node.Q<ElementsTable>();
                        var field = new NewspaperElementsRowField();
                        field.Set(AaKeys.LanguageKeys[0], onChange: node.CheckNodeContent);
                        tableContainer?.Add(field);
                        node.CheckNodeContent();
                    }

                    break;
                case LanguageOperationType.Change:
                    foreach (var node in phraseNodes)
                    {
                        ChangeRow<PhraseElementsRowField>(node);
                    }

                    foreach (var node in newspaperNodes)
                    {
                        ChangeRow<NewspaperElementsRowField>(node);
                    }

                    void ChangeRow<T>(AaNode node) where T : VisualElement
                    {
                        var rows = node.Query<T>().ToList();
                        var row = rows[index];
                        var label = row.Q<Label>();
                        label.text = languageOperation.Value;
                    }

                    break;

                case LanguageOperationType.Remove:
                    foreach (var node in phraseNodes)
                    {
                        RemoveRow<PhraseElementsRowField>(node);
                        node.CheckNodeContent();
                    }
                    
                    foreach (var node in newspaperNodes)
                    {
                        RemoveRow<NewspaperElementsRowField>(node);
                        node.CheckNodeContent();
                    }
                    
                    entryNode.Remove(languageOperation.Element);

                    void RemoveRow<T>(AaNode node) where T : VisualElement
                    {
                        var rows = node.Query<T>().ToList();
                        var row = rows[index];
                        var phraseContainer = node.Q<ElementsTable>();
                        phraseContainer.Remove(row);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}