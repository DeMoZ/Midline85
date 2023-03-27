using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AaDialogueGraph.Editor
{
    public partial class GraphSaveUtility
    {
        private const string NoGraphName = "NoGraph";
        private DialogueGraphView _targetGraphView;
        private DialogueContainer _containerCash;

        private AaReactive<LanguageOperation> _languageOperation;

        private List<Edge> Edges => _targetGraphView.edges.ToList();
        private List<AaNode> AaNodes => _targetGraphView.Query<AaNode>().ToList();

        public static AaDialogueGraph.Editor.GraphSaveUtility GetInstance(DialogueGraphView targetGraphView,
            AaReactive<LanguageOperation> languageOperation)
        {
            return new AaDialogueGraph.Editor.GraphSaveUtility
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

            var entryNode = AaNodes.Select(n => n as EntryPointNode).ToList();
            if (entryNode.Any())
            {
                var node = entryNode.First(n => n != null);
                dialogueContainer.EntryGuid = node.Guid;
                dialogueContainer.EntryRect = node.GetPosition();
            }

            var phraseNodes = AaNodes.OfType<PhraseNode>().ToList();
            dialogueContainer.PhraseNodeData.AddRange(PhraseNodesToData(phraseNodes));

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
                var phraseSounds = node.GetPhraseSounds().Cast<Object>().ToList();
                var phrases = node.GetPhrases().Cast<Object>().ToList();

                data.Add(new PhraseNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),

                    PhraseSketchText = node.PhraseSketchText,
                    PersonVisualData = personVisualData,
                    PhraseVisualData = phraseVisualData,
                    EventVisualData = eventsVisualData,
                    PhraseSounds = NodeUtils.GetObjectPath(phraseSounds),
                    Phrases = NodeUtils.GetObjectPath(phrases),
                });
            }

            return data;
        }

        private List<ChoiceNodeData> ChoiceNodesToData(List<ChoiceNode> nodes)
        {
            var data = new List<ChoiceNodeData>();
            foreach (var node in nodes)
            {
                var andCases = node.Query<AndChoiceCase>().ToList();
                var noCases = node.Query<NoChoiceCase>().ToList();

                var caseData = new List<CaseData>();

                foreach (var andCase in andCases)
                {
                    caseData.Add(new CaseData
                    {
                        And = true,
                        Cases = andCase.GetOrCases(),
                    });
                }

                foreach (var noCase in noCases)
                {
                    caseData.Add(new CaseData
                    {
                        And = false,
                        Cases = noCase.GetOrCases(),
                    });
                }

                data.Add(new ChoiceNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),

                    Choice = node.Q<ChoicePopupField>().Value,
                    Cases = caseData,
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