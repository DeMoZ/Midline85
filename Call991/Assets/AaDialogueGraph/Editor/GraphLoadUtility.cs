using System.Collections.Generic;
using System.IO;
using System.Linq;
using Configs;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace AaDialogueGraph.Editor
{
    public partial class GraphSaveUtility
    {
        private SoundLists _soundLists;

        public bool LoadGraph(ref string path)
        {
            var fileName = EditorUtility.OpenFilePanel("Dialogue Graph", "Assets/Resources/", "asset");

            if (string.IsNullOrEmpty(fileName))
            {
                path = NoGraphName;
                return false;
            }

            path = Path.GetDirectoryName(fileName);
            path = path.Replace("\\", "/");
            var onlyFileName = Path.GetFileNameWithoutExtension(fileName);

            var split = path.Split("Resources/");
            path = split.Length > 1 ? Path.Combine(split[1], onlyFileName) : onlyFileName;

            _containerCash = Resources.Load<DialogueContainer>(path);

            if (!_containerCash)
            {
                EditorUtility.DisplayDialog("File not found", $"Target dialogue {path} doesn't exist", "OK");
                path = NoGraphName;
                return false;
            }

            var gameSet = Resources.Load<GameSet>("GameSet");
            _soundLists = new SoundLists
            {
                Voices = gameSet.VoicesSet.GetKeys(),
                Sfxs = gameSet.SfxsSet.GetKeys(),
                Musics = gameSet.MusicSwitchesKeys.GetKeys(),
                Rtcps = gameSet.RtpcKeys.GetKeys(),
            };

            ClearGraph();
            CreateEntryNode();
            CreatePhraseNodes();
            CreateImagePhraseNodes();
            CreateChoiceNodes();
            CreateForkNodes();
            CreateCountNodes();
            CreateEndNodes();
            CreateEventNodes();
            CreateNewspaperNodes();
            CreateSlideNodes();

            ConnectNodes();
            SetPositions();

            return true;
        }

        private void ClearGraph()
        {
            foreach (var node in AaNodes)
            {
                Enumerable.Where<Edge>(Edges, x => x.input.node == node).ToList()
                    .ForEach(edge => _targetGraphView.RemoveElement(edge));
                _targetGraphView.RemoveElement(node);
            }
        }

        private void CreateEntryNode()
        {
            var node = new EntryNode();
            node.Set(_languageOperation, _containerCash.EntryNodeData);
            _targetGraphView.AddElement(node);

            foreach (var language in _containerCash.EntryNodeData.Languages)
            {
                var field = new LanguageField();
                field.Set(language, _languageOperation);
                node.contentContainer.Add(field);
            }
        }

        // private List<string> GetWwiseSoundsFromStartNode()
        // {
        //     return (string.IsNullOrEmpty(_containerCash.EntryNodeData.SoundAsset) ? null : 
        //         NodeUtils.GetObjectByPath<WwiseSoundsKeysList>(_containerCash.EntryNodeData.SoundAsset))?.Keys;
        // }

        private void CreatePhraseNodes()
        {
            var languages = _containerCash.EntryNodeData.Languages;

            foreach (var data in _containerCash.PhraseNodeData)
            {
                var node = new PhraseNode();
                node.Set(data, languages, _soundLists, data.Guid);
                _targetGraphView.AddElement(node);
            }
        }

        private void CreateImagePhraseNodes()
        {
            var languages = _containerCash.EntryNodeData.Languages;

            foreach (var data in _containerCash.ImagePhraseNodeData)
            {
                var node = new ImagePhraseNode();
                node.Set(data, languages, _soundLists, data.Guid);
                _targetGraphView.AddElement(node);
            }
        }

        private void CreateChoiceNodes()
        {
            var choiceKeys = EditorNodeUtils.GetButtons(_containerCash.EntryNodeData.ButtonFilter);

            foreach (var data in _containerCash.ChoiceNodeData)
            {
                var node = new ChoiceNode();
                node.Set(data, data.Guid, choiceKeys);
                _targetGraphView.AddElement(node);
            }
        }

        private void CreateForkNodes()
        {
            foreach (var data in _containerCash.ForkNodeData)
            {
                var node = new ForkNode();
                node.Set(data, data.Guid);
                _targetGraphView.AddElement(node);
            }
        }

        private void CreateCountNodes()
        {
            foreach (var data in _containerCash.CountNodeData)
            {
                var node = new CountNode();
                node.Set(data, data.Guid);
                _targetGraphView.AddElement(node);
            }
        }

        private void CreateEndNodes()
        {
            foreach (var data in _containerCash.EndNodeData)
            {
                var node = new EndNode();
                node.Set(data, data.Guid, _soundLists);
                _targetGraphView.AddElement(node);
            }
        }

        private void CreateEventNodes()
        {
            foreach (var data in _containerCash.EventNodeData)
            {
                var node = new EventNode();
                node.Set(data, data.Guid, _soundLists);
                _targetGraphView.AddElement(node);
            }
        }

        private void CreateNewspaperNodes()
        {
            var languages = _containerCash.EntryNodeData.Languages;
            
            foreach (var data in _containerCash.NewspaperNodeData)
            {
                var node = new NewspaperNode();
                node.Set(data, languages, data.Guid, _soundLists);
                _targetGraphView.AddElement(node);
            }
        } 
        
        private void CreateSlideNodes()
        {
            var languages = _containerCash.EntryNodeData.Languages;
            
            foreach (var data in _containerCash.SlideNodeData)
            {
                var node = new SlideNode();
                node.Set(data, languages, data.Guid, _soundLists);
                _targetGraphView.AddElement(node);
            }
        }

        private void ConnectNodes()
        {
            foreach (var node in AaNodes)
            {
                var nodeLinkData = _containerCash.NodeLinks.Where(x => x.BaseNodeGuid == node.Guid).ToList();
                var outPort = UQueryExtensions.Q<Port>(node.outputContainer); // port and inheritances

                for (var j = 0; j < nodeLinkData.Count; j++)
                {
                    var targetNodeGuid = nodeLinkData[j].TargetNodeGuid;
                    var targetNode = Enumerable.FirstOrDefault<AaNode>(AaNodes, x => x.Guid == targetNodeGuid);

                    if (targetNode == null) continue;

                    var portIn = (Port)targetNode.inputContainer[0];
                    Port portOut = string.IsNullOrEmpty(nodeLinkData[j].BaseExitName)
                        ? outPort
                        : node.Query<Port>().ToList().FirstOrDefault(p => p.name == nodeLinkData[j].BaseExitName);

                    LinkNodes(portOut, portIn);
                }
            }
        }

        private void LinkNodes(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input,
            };

            tempEdge.input?.Connect(tempEdge);
            tempEdge.output?.Connect(tempEdge);
            _targetGraphView.Add(tempEdge);
        }

        private void SetPositions()
        {
            var nodes = new List<AaNodeData>();

            nodes.AddRange(_containerCash.PhraseNodeData);
            nodes.AddRange(_containerCash.ImagePhraseNodeData);
            nodes.AddRange(_containerCash.ChoiceNodeData);
            nodes.AddRange(_containerCash.ForkNodeData);
            nodes.AddRange(_containerCash.CountNodeData);
            nodes.AddRange(_containerCash.EventNodeData);
            nodes.AddRange(_containerCash.EndNodeData);
            nodes.AddRange(_containerCash.NewspaperNodeData);
            nodes.AddRange(_containerCash.SlideNodeData);
            nodes.Add(_containerCash.EntryNodeData);

            // foreach (var node in AaNodes)
            // {
            //     var data = nodes.FirstOrDefault(n => n.Guid == node.Guid);
            //     if (data != null) node.SetPosition(data.Rect);
            // }

            var nodesDict = nodes.ToDictionary(nodeData => nodeData.Guid);

            foreach (var node in AaNodes)
            {
                if (nodesDict.TryGetValue(node.Guid, out var data))
                {
                    node.SetPosition(data.Rect);
                }
            }
        }
    }
}