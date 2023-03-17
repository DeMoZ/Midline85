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
        public bool LoadGraph(ref string path)
        {
            var fileName = EditorUtility.OpenFilePanel("Dialogue Graph", "Assets/Resources/", "asset");

            if (string.IsNullOrEmpty(fileName))
            {
                path = NoGraphName;
                return false;
            }

            path = Path.GetDirectoryName(fileName);
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

            ClearGraph();
            CreateEntryPoint();
            CreatePhraseNodes();
            CreateChoiceNodes();
            ConnectNodes();
            SetPositions();

            return true;
        }

        private void ClearGraph()
        {
            foreach (var node in AaNodes)
            {
                Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
                _targetGraphView.RemoveElement(node);
            }
        }

        private void CreateEntryPoint()
        {
            var entryNode = new EntryPointNode(_languageOperation, _containerCash.EntryGuid);
            _targetGraphView.AddElement(entryNode);

            foreach (var language in _containerCash.Languages)
            {
                entryNode.contentContainer.Add(new LanguageField(language, _languageOperation));
            }
        }

        private void CreatePhraseNodes()
        {
            foreach (var data in _containerCash.PhraseNodeData)
            {
                var node = new PhraseNode(data, _containerCash.Languages, data.Guid);
                _targetGraphView.AddElement(node);
            }
        }

        private void CreateChoiceNodes()
        {
            foreach (var data in _containerCash.ChoiceNodeData)
            {
                var dt = data;
                var node = new ChoiceNode(data, data.Guid);
                _targetGraphView.AddElement(node);
            }
        }

        private void ConnectNodes()
        {
            foreach (var node in AaNodes)
            {
                var nodeLinkData = _containerCash.NodeLinks.Where(x => x.BaseNodeGuid == node.Guid).ToList();
                var outPort = node.outputContainer.Q<Port>();

                for (var j = 0; j < nodeLinkData.Count; j++)
                {
                    var targetNodeGuid = nodeLinkData[j].TargetNodeGuid;
                    var targetNode = AaNodes.FirstOrDefault(x => x.Guid == targetNodeGuid);

                    if (targetNode == null) continue;

                    var portOut = outPort;
                    var portIn = (Port) targetNode.inputContainer[0];

                    // link
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
            foreach (var node in AaNodes)
            {
                switch (node)
                {
                    case EntryPointNode:
                        node.SetPosition(_containerCash.EntryRect);
                        break;

                    case PhraseNode:
                        var phraseData = _containerCash.PhraseNodeData.FirstOrDefault(n => n.Guid == node.Guid);
                        if (phraseData != null) node.SetPosition(phraseData.Rect);
                        break;

                    case ChoiceNode:
                    {
                        var choiceData = _containerCash.ChoiceNodeData.FirstOrDefault(n => n.Guid == node.Guid);
                        if (choiceData != null) node.SetPosition(choiceData.Rect);
                        break;
                    }
                }
            }
        }
    }
}