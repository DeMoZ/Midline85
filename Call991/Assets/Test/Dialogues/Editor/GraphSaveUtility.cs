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
    public class GraphSaveUtility
    {
        private const string NoGraphName = "NoGraph";
        private DialogueGraphView _targetGraphView;
        private DialogueContainer _containerCash;

        private List<Edge> Edges => _targetGraphView.edges.ToList();
        private List<PhraseNode> Nodes => _targetGraphView.nodes.ToList().Cast<PhraseNode>().ToList();

        public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
        {
            return new GraphSaveUtility()
            {
                _targetGraphView = targetGraphView,
            };
        }

        public void SaveGraph(string fileName)
        {
            if (!Edges.Any()) return;

            var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
            dialogueContainer.Languages = Nodes.First(n => n.EntryPoint).Languages;
            
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

            foreach (var dialogueNode in Nodes.Where(node => !node.EntryPoint))
            {
                dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
                {
                    Guid = dialogueNode.Guid,
                    DialogueText = dialogueNode.DialogueText,
                    Position = dialogueNode.GetPosition().position,
                    Size = dialogueNode.GetPosition().size,
                });
            }

            CreateFolders(fileName);

            AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
            AssetDatabase.SaveAssets();

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

        public string LoadGraph()
        {
            var fileName = EditorUtility.OpenFilePanel("Dialogue Graph", "Assets/Resources/", "asset");
            
            if (string.IsNullOrEmpty(fileName))
            {
                return NoGraphName;
            }

            var path = Path.GetDirectoryName(fileName);
            var relativePath = path.Split("Resources/")[1];
            var onlyFileName = Path.GetFileNameWithoutExtension(fileName);
            
            path = Path.Combine(relativePath, onlyFileName);
            _containerCash = Resources.Load<DialogueContainer>(path);

            if (!_containerCash)
            {
                EditorUtility.DisplayDialog("File not found", $"Target dialogue {path} doesn't exist", "OK");
                return NoGraphName;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();

            return path;
        }
        
        private void ClearGraph()
        {
            foreach (var node in Nodes)
            {
                if (node.EntryPoint)
                {
                    node.Guid = _containerCash.NodeLinks[0].BaseNodeGuid;
                    continue;
                }

                Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
                _targetGraphView.RemoveElement(node);
            }
        }

        private void CreateNodes()
        {
            foreach (var node in _containerCash.DialogueNodeData)
            {
                var tmpNode = _targetGraphView.CreatePhraseNode(node.DialogueText);
                tmpNode.Guid = node.Guid;
                tmpNode.SetPosition(new Rect(node.Position, node.Size));

                _targetGraphView.AddElement(tmpNode);

                var ports = _containerCash.NodeLinks.Where(x => x.BaseNodeGuid == node.Guid).ToList();
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