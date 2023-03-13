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
        private List<PhraseNode> Nodes => _targetGraphView.nodes.ToList().Cast<PhraseNode>().ToList();

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

            var entryContainer = Nodes.First(n => n.EntryPoint).contentContainer;
            var languageFields = entryContainer.Query<LanguageField>().ToList();
            languageFields.ForEach(lf => dialogueContainer.Languages.Add(lf.Language));

            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();

            foreach (var port in connectedPorts)
            {
                var inputNode = port.input.node as PhraseNode;
                var outputNode = port.output.node as PhraseNode;

                dialogueContainer.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGuid = outputNode!.Guid,
                    PortName = port.output.portName,
                    TargetNodeGuid = inputNode!.Guid,
                });
            }

            dialogueContainer.EntryGuid = Nodes.First(node => node.EntryPoint).Guid;
            
            foreach (var phraseNode in Nodes.Where(node => !node.EntryPoint))
            {
                var personVisualData = phraseNode.GetPersonVisual().GetData();
                var phraseVisualData = phraseNode.GetPhraseVisual().GetData();
                var eventsVisualData = phraseNode.GetEventsVisual().Select(evt => evt.GetData()).ToList();
                var phraseSounds = phraseNode.GetPhraseSounds();
                var phrases = phraseNode.GetPhrases();
                
                dialogueContainer.DialogueNodeData.Add(new PhraseNodeData
                {
                    Guid = phraseNode.Guid,
                    DialogueText = phraseNode.DialogueText,
                    Position = phraseNode.GetPosition().position,
                    Size = phraseNode.GetPosition().size,
                    PersonVisualData = personVisualData,
                    PhraseVisualData = phraseVisualData,
                    EventVisualData = eventsVisualData,
                    PhraseSounds = phraseSounds,
                    Phrases = phrases,
                });
            }

            CreateFolders(fileName);

            AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Dialogue <color=yellow>{fileName}</color> Saved. {DateTime.Now}");
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