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

            var entryNode = AaNodes.FirstOrDefault(n => n is EntryNode);
            if (entryNode != null)
            {
                dialogueContainer.EntryNodeData = EntryNodeToData(entryNode as EntryNode);
            }

            var phraseNodes = AaNodes.OfType<PhraseNode>().ToList();
            dialogueContainer.PhraseNodeData.AddRange(PhraseNodesToData(phraseNodes));

            var imagePhraseNodes = AaNodes.OfType<ImagePhraseNode>().ToList();
            dialogueContainer.ImagePhraseNodeData.AddRange(ImagePhraseNodesToData(imagePhraseNodes));

            var choiceNodes = AaNodes.OfType<ChoiceNode>().ToList();
            dialogueContainer.ChoiceNodeData.AddRange(ChoiceNodesToData(choiceNodes));

            var forkNodes = AaNodes.OfType<ForkNode>().ToList();
            dialogueContainer.ForkNodeData.AddRange(ForkNodesToData(forkNodes));

            var countNodes = AaNodes.OfType<CountNode>().ToList();
            dialogueContainer.CountNodeData.AddRange(CountNodesToData(countNodes));

            var endNodes = AaNodes.OfType<EndNode>().ToList();
            dialogueContainer.EndNodeData.AddRange(EndNodesToData(endNodes));

            var eventNodes = AaNodes.OfType<EventNode>().ToList();
            dialogueContainer.EventNodeData.AddRange(EventNodesToData(eventNodes));

            var newspaperNodes = AaNodes.OfType<NewspaperNode>().ToList();
            dialogueContainer.NewspaperNodeData.AddRange(NewspaperNodesToData(newspaperNodes));
            
            var slideNodes = AaNodes.OfType<SlideNode>().ToList();
            dialogueContainer.SlideNodeData.AddRange(SlideNodesToData(slideNodes));

            var assetName = $"Assets/Resources/{fileName}.asset";
            if (File.Exists(assetName))
            {
                var theFile = Resources.Load<DialogueContainer>(fileName);
                EditorUtility.SetDirty(theFile);
                EditorUtility.CopySerialized(dialogueContainer, theFile);
                theFile.name = Path.GetFileName(fileName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Dialogue <color=yellow>{fileName}</color> <color=green>Refreshed</color>. {DateTime.Now}");
            }
            else
            {
                CreateFolders(fileName);

                AssetDatabase.CreateAsset(dialogueContainer, assetName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Dialogue <color=yellow>{fileName}</color> Saved. {DateTime.Now}");
            }
        }

        private EntryNodeData EntryNodeToData(EntryNode node)
        {
            var data = new EntryNodeData
            {
                Guid = node.Guid,
                Rect = node.GetPosition(),
                LevelId = node.Q<LevelIdPopupField>().Value,
                ButtonFilter = node.Q<ButtonFilterTextField>().value,
                GrabProjectorImages = node.Q<Toggle>(AaGraphConstants.ProjectorImages).value,
                EnableSkipLevelButton = node.Q<Toggle>(AaGraphConstants.EnableSkipLevelButton).value,
            };

            var languageFields = node.Query<LanguagePopupField>().ToList();
            data.Languages = languageFields.Select(field => field.Value).ToList();
            return data;
        }

        private List<PhraseNodeData> PhraseNodesToData(List<PhraseNode> nodes)
        {
            var data = new List<PhraseNodeData>();
            foreach (var node in nodes)
            {
                var personVisualData = node.GetPersonVisual().GetData();
                var phraseVisualData = node.GetPhraseVisual().GetData();
                var eventsVisualData = GetEventsData(node);
                var phraseSound = node.GetPhraseSound();
                var phrases = node.GetPhrases().Cast<Object>().ToList();

                data.Add(new PhraseNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),

                    PhraseSketchText = node.PhraseSketchText,
                    PersonVisualData = personVisualData,
                    PhraseVisualData = phraseVisualData,
                    EventVisualData = eventsVisualData,
                    PhraseSound = phraseSound,
                    Phrases = EditorNodeUtils.GetPathByObjects(phrases),
                });
            }

            return data;
        }
        
        private List<ImagePhraseNodeData> ImagePhraseNodesToData(List<ImagePhraseNode> nodes)
        {
            var data = new List<ImagePhraseNodeData>();
            foreach (var node in nodes)
            {
                var personVisualData = node.GetImagePersonVisual().GetData();
                var phraseVisualData = node.GetPhraseVisual().GetData();
                var eventsVisualData = GetEventsData(node);
                var phraseSound = node.GetPhraseSound();
                var phrases = node.GetPhrases().Cast<Object>().ToList();

                data.Add(new ImagePhraseNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),

                    PhraseSketchText = node.PhraseSketchText,
                    ImagePersonVisualData = personVisualData,
                    PhraseVisualData = phraseVisualData,
                    EventVisualData = eventsVisualData,
                    PhraseSound = phraseSound,
                    Phrases = EditorNodeUtils.GetPathByObjects(phrases),
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
                    ForceSelectOnRandom = node.Q<Toggle>(AaGraphConstants.ForceChoice).value,
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
                var eventsVisualData = GetEventsData(node);

                data.Add(new EndNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),
                    End = node.Q<EndPopupField>().Value,
                    EventVisualData = eventsVisualData,
                    SkipSelectNextLevelButtons = node.Q<Toggle>(AaGraphConstants.EndNodeSkipSelectNextLevelButtons).value
                    //Records = node.GetRecords(),
                });
            }

            return data;
        }

        private List<EventNodeData> EventNodesToData(List<EventNode> nodes)
        {
            var data = new List<EventNodeData>();
            foreach (var node in nodes)
            {
                var eventsVisualData = GetEventsData(node);
                data.Add(new EventNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),
                    EventVisualData = eventsVisualData,
                });
            }

            return data;
        }

        private List<NewspaperNodeData> NewspaperNodesToData(List<NewspaperNode> nodes)
        {
            var data = new List<NewspaperNodeData>();
            foreach (var node in nodes)
            {
                var eventsVisualData = GetEventsData(node);
                var newspaper = node.Q<NewspaperAssetField>(AaGraphConstants.NewspaperObject).GetNewspaper();

                data.Add(new NewspaperNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),
                    EventVisualData = eventsVisualData,
                    NewspaperPrefab = EditorNodeUtils.GetPathByObject(newspaper),
                });
            }

            return data;
        }

        private List<SlideNodeData> SlideNodesToData(List<SlideNode> nodes)
        {
            var data = new List<SlideNodeData>();
            foreach (var node in nodes)
            {
                var eventsVisualData = GetEventsData(node);
                var slides = node.GetSlides().Cast<Object>().ToList();

                data.Add(new SlideNodeData
                {
                    Guid = node.Guid,
                    Rect = new Rect(node.GetPosition().position, node.GetPosition().size),
                    
                    EventVisualData = eventsVisualData,
                    Slides = EditorNodeUtils.GetPathByObjects(slides),
                });
            }

            return data;
        }

        private static List<EventVisualData> GetEventsData(AaNode node)
        {
            return node.GetEventsVisual().ToList().Select(evt => evt.GetData()).ToList();
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