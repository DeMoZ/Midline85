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
        private readonly List<string> _voices;
        private readonly List<string> _musics;
        private readonly List<string> _rtcps;

        public DialogueGraphView(AaReactive<LanguageOperation> languageOperation, 
            List<string> voices, List<string> musics, List<string> rtpcs)
        {
            _voices = voices;
            _musics = musics;
            _rtcps = rtpcs;
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
            var entryNodeData = new EntryNodeData();
            var node = new EntryNode();
            node.Set(languageOperation, entryNodeData);
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
            var languages = GetLanguages();
            var sounds = new List<string>();
            var node = new PhraseNode();
            node.Set(new PhraseNodeData(), languages, _voices, _musics, _rtcps, sounds, Guid.NewGuid().ToString());
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));

            AddElement(node);
        }

        public void CreateImagePhraseNode()
        {
            var languages = GetLanguages();
            var sounds = new List<string>();
            var node = new ImagePhraseNode();
            node.Set(new ImagePhraseNodeData(), languages, _voices, _musics, _rtcps, sounds, Guid.NewGuid().ToString());
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));

            AddElement(node);
        }

        public void CreateChoiceNode()
        {
            var node = new ChoiceNode();
            node.Set(new ChoiceNodeData(), Guid.NewGuid().ToString(),
                EditorNodeUtils.GetButtons(contentContainer.Q<EntryNode>().GetFilters()));
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
            var sounds = new List<string>();
            var node = new EventNode();
            node.Set(new EventNodeData(), Guid.NewGuid().ToString(), sounds, _musics, _rtcps);
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));
            AddElement(node);
        }

        public void CreateEndNode()
        {
            var sounds = new List<string>();
            var node = new EndNode();
            node.Set(new EndNodeData(), Guid.NewGuid().ToString(), sounds, _musics, _rtcps);
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));
            AddElement(node);
        }

        public void CreateNewspaperNode()
        {
            var languages = GetLanguages();
            var sounds = new List<string>();
            var node = new NewspaperNode();
            node.Set(new NewspaperNodeData(), languages, Guid.NewGuid().ToString(), sounds, _musics, _rtcps);
            node.SetPosition(new Rect(GetNewNodePosition(), Vector2.zero));

            AddElement(node);
        }

        private List<string> GetLanguages() => contentContainer.Q<EntryNode>().GetLanguages() ?? new List<string>();

        // private List<string> GetSoundAsset()
        // {
        //     var keys = contentContainer.Q<SoundAssetField>().GetSoundAsset();
        //     return keys != null ? keys.Keys : new List<string>();
        // }

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
                        field.Set(AaKeys.LanguageKeys[0], null, node.CheckNodeContent);
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