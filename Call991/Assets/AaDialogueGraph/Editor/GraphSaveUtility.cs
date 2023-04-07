using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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
            languageFields.ForEach(lf => dialogueContainer.EntryNodeData.Languages.Add(lf.Language));

            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();

            foreach (var port in connectedPorts)
            {
                var inputNode = port.input.node as AaNode;
                var outputNode = port.output.node as AaNode;

                dialogueContainer.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGuid = outputNode!.Guid,
                    BaseExitName = port.output.name,
                    TargetNodeGuid = inputNode!.Guid,
                });
            }

            var entryNode = AaNodes.Select(n => n as EntryPointNode).ToList();
            if (entryNode.Any())
            {
                var node = entryNode.First(n => n != null);
                dialogueContainer.EntryNodeData.Guid = node.Guid;
                dialogueContainer.EntryNodeData.Rect = node.GetPosition();
            }

            var phraseNodes = AaNodes.OfType<PhraseNode>().ToList();
            dialogueContainer.PhraseNodeData.AddRange(PhraseNodesToData(phraseNodes));

            var choiceNodes = AaNodes.OfType<ChoiceNode>().ToList();
            dialogueContainer.ChoiceNodeData.AddRange(ChoiceNodesToData(choiceNodes));

            var forkNodes = AaNodes.OfType<ForkNode>().ToList();
            dialogueContainer.ForkNodeData.AddRange(ForkNodesToData(forkNodes));

            var countNodes = AaNodes.OfType<CountNode>().ToList();
            dialogueContainer.CountNodeData.AddRange(CountNodesToData(countNodes));

            var endNodes = AaNodes.OfType<EndNode>().ToList();
            dialogueContainer.EndNodeData.AddRange(EndNodesToData(endNodes));

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
                    PhraseSounds = EditorNodeUtils.GetObjectPath(phraseSounds),
                    Phrases = EditorNodeUtils.GetObjectPath(phrases),
                });
            }

            return data;
        }

        private List<ChoiceNodeData> ChoiceNodesToData(List<ChoiceNode> nodes)
        {
            var data = new List<ChoiceNodeData>();
            foreach (var node in nodes)
            {
                var caseData = GetCaseData(node);

                data.Add(new ChoiceNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),
                    Choice = node.Q<ChoicePopupField>().Value,
                    CaseData = caseData,
                });
            }

            return data;
        }

        private List<ForkNodeData> ForkNodesToData(List<ForkNode> nodes)
        {
            var data = new List<ForkNodeData>();

            foreach (var node in nodes)
            {
                var exits = new List<ForkCaseData>();
                var prongs = node.Query<ForkExitElement>().ToList();
                
                foreach (var prong in prongs)
                {
                    exits.Add(new ForkCaseData(GetCaseData(prong), prong.Guid));
                }

                data.Add(new ForkNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),
                    ForkCaseData = exits,
                });
            }

            return data;
        }

        private List<CountNodeData> CountNodesToData(List<CountNode> nodes)
        {
            var data = new List<CountNodeData>();
            foreach (var node in nodes)
            {
                data.Add(new CountNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),
                    Choice = node.Q<CountPopupField>().Value,
                    Value = node.Q<IntegerField>().value,
                });
            }

            return data;
        }

        private List<EndNodeData> EndNodesToData(List<EndNode> nodes)
        {
            var data = new List<EndNodeData>();
            foreach (var node in nodes)
            {
                data.Add(new EndNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),
                    End = node.Q<EndPopupField>().Value,
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

        private CaseData GetCaseData(VisualElement container)
        {
            var words = GetChoiceCases(container);
            var ends = GetEndCases(container);
            var counts = GetCountCases(container);

            return new CaseData(words, ends, counts);
        }

        private List<ChoiceData> GetChoiceCases(VisualElement container)
        {
            var result = new List<ChoiceData>();
            var andWordCases = container.Query<AndChoiceCase>().ToList();
            var noWordCases = container.Query<NoChoiceCase>().ToList();

            foreach (var andCase in andWordCases)
            {
                result.Add(new ChoiceData
                {
                    CaseType = CaseType.AndWord,
                    OrKeys = andCase.GetOrCases(),
                });
            }

            foreach (var noCase in noWordCases)
            {
                result.Add(new ChoiceData
                {
                    CaseType = CaseType.NoWord,
                    OrKeys = noCase.GetOrCases(),
                });
            }

            return result;
        }

        private List<EndData> GetEndCases(VisualElement container)
        {
            var result = new List<EndData>();
            var andEndCases = container.Query<AndEndCase>().ToList();
            var noEndCases = container.Query<NoEndCase>().ToList();
            
            foreach (var andCase in andEndCases)
            {
                result.Add(new EndData
                {
                    EndType = EndType.AndEnd,
                    OrKeys = andCase.GetOrCases(),
                });
            }

            foreach (var noCase in noEndCases)
            {
                result.Add(new EndData
                {
                    EndType = EndType.NoEnd,
                    OrKeys = noCase.GetOrCases(),
                });
            }

            return result;
        }

        private List<CountData> GetCountCases(VisualElement container)
        {
            var result = new List<CountData>();
            var countCases = container.Query<CountCase>().ToList();
            
            foreach (var countCase in countCases)
            {
                result.Add(new CountData
                {
                    CountType = CountType.Sum,
                    CountKey = countCase.GetKey(),
                    Range = countCase.GetRange(),
                });
            }
           
            return result;
        }
    }
}