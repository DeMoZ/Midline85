using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Dialogues
{
    public partial class GraphSaveUtility
    {
        private const string NoGraphName = "NoGraph";
        private DialogueGraphView _targetGraphView;
        private DialogueContainer _containerCash;

        private AaReactive<LanguageOperation> _languageOperation;

        private List<Edge> Edges => _targetGraphView.edges.ToList();
        private List<AaNode> AaNodes => _targetGraphView.Query<AaNode>().ToList();

        public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView,
            AaReactive<LanguageOperation> languageOperation)
        {
            return new GraphSaveUtility
            {
                _targetGraphView = targetGraphView,
                _languageOperation = languageOperation,
            };
        }

        public void SaveGraph(string fileName)
        {
            var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();

            var entryContainer = AaNodes.First(n => n.EntryPoint).contentContainer;
            var languageFields = entryContainer.Query<LanguageField>().ToList();
            languageFields.ForEach(lf => dialogueContainer.Languages.Add(lf.Language));

            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();

            foreach (var port in connectedPorts)
            {
                var inputNode = port.input.node as AaNode;
                var outputNode = port.output.node as AaNode;

                dialogueContainer.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGuid = outputNode!.Guid,
                    TargetNodeGuid = inputNode!.Guid,
                });
            }

            dialogueContainer.EntryGuid = AaNodes.First(node => node.EntryPoint).Guid;

            var phraseNodes = AaNodes.OfType<PhraseNode>().ToList();
            dialogueContainer.DialogueNodeData.AddRange(PhraseNodesToData(phraseNodes));

            var choiceNodes = AaNodes.OfType<ChoiceNode>().ToList();
            dialogueContainer.ChoiceNodeData.AddRange(ChoiceNodesToData(choiceNodes));

            CreateFolders(fileName);

            AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Dialogue <color=yellow>{fileName}</color> Saved. {DateTime.Now}");
        }

        private List<PhraseNodeData> PhraseNodesToData(List<PhraseNode> nodes)
        {
            var data = new List<PhraseNodeData>();
            foreach (var node in nodes)
            {
                var personVisualData = node.GetPersonVisual().GetData();
                var phraseVisualData = node.GetPhraseVisual().GetData();
                var eventsVisualData = node.GetEventsVisual().Select(evt => evt.GetData()).ToList();
                var phraseSounds = node.GetPhraseSounds();
                var phrases = node.GetPhrases();

                data.Add(new PhraseNodeData
                {
                    Guid = node.Guid,
                    PhraseSketchText = node.PhraseSketchText,
                    Position = node.GetPosition().position,
                    Size = node.GetPosition().size,
                    PersonVisualData = personVisualData,
                    PhraseVisualData = phraseVisualData,
                    EventVisualData = eventsVisualData,
                    PhraseSounds = phraseSounds,
                    Phrases = phrases,
                });
            }

            return data;
        }

        private List<ChoiceNodeData> ChoiceNodesToData(List<ChoiceNode> nodes)
        {
            var data = new List<ChoiceNodeData>();
            foreach (var node in nodes)
            {
                data.Add(new ChoiceNodeData
                {
                    Choice = node.Q<ChoiceNode.ChoicePopupField>().Value,
                    Cases = ,
                });
            }

            return data;
        }

        private void CreateFolders(string fileName)
        {
            var pathName = $"Resources/{fileName}";
            var pathList = pathName.Split("/");

            var pathPart = "Assets";

            for (var i = 0; i < pathList.Length - 1; i++)
            {
                var part = Path.Combine(pathPart, pathList[i]);

                if (!AssetDatabase.IsValidFolder(part))
                    AssetDatabase.CreateFolder(pathPart, pathList[i]);

                pathPart = part;
            }
        }
    }
}