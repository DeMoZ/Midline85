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
            CreateNodes();
            ConnectNodes();

            return true;
        }
        
        private void ClearGraph()
        {
            foreach (var node in Nodes)
            {
                // if (node.EntryPoint)
                // {
                //     node.Guid = _containerCash.NodeLinks[0].BaseNodeGuid;
                //     continue;
                // }

                Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
                _targetGraphView.RemoveElement(node);
            }
        }
        
        private void CreateEntryPoint()
        {
            var entryNode = new EntryPointNode();
            //entryNode.style.width = 200;
            
            _targetGraphView.AddElement(entryNode);

            foreach (var language in _containerCash.Languages)
            {
                var languageField = new LanguageField(language,
                    onDelete: obj => { entryNode.contentContainer.Remove(obj); });

                entryNode.contentContainer.Add(languageField);
            }
        }
        
        private void CreateNodes()
        {
            foreach (var nodeData in _containerCash.DialogueNodeData)
            {
                var tmpNode = _targetGraphView.CreatePhraseNode(nodeData, _containerCash.Languages );//nodeData.DialogueText, _containerCash.Languages ,nodeData.Phrases);
                tmpNode.Guid = nodeData.Guid;
                tmpNode.SetPosition(new Rect(nodeData.Position, nodeData.Size));

                _targetGraphView.AddElement(tmpNode);

                var ports = _containerCash.NodeLinks.Where(x => x.BaseNodeGuid == nodeData.Guid).ToList();
                ports.ForEach(x => _targetGraphView.AddChoicePort(tmpNode, x.PortName));
            }
        }

        private void ConnectNodes()
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                var connections = _containerCash.NodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].Guid).ToList();

                for (var j = 0; j < connections.Count; j++)
                {
                    var targetNodeGuid = connections[j].TargetNodeGuid;
                    var targetNode = Nodes.First(x => x.Guid == targetNodeGuid);
                    LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port) targetNode.inputContainer[0]);
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
    }
}