using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Test.Dialogues
{
    public class GraphSaveUtility
    {
        private DialogueGraphView _targetGraphView;
        private DialogueContainer _containerCash;

        private List<Edge> Edges => _targetGraphView.edges.ToList();
        private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

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
            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();

            foreach (var port in connectedPorts)
            {
                var inputNode = port.input.node as DialogueNode;
                var outputNode = port.output.node as DialogueNode;

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
                });
            }

            CreateFolders(fileName);

            AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
            AssetDatabase.SaveAssets();

            Debug.Log($"Dialogue <color=yellow>{fileName}</color> Saved");
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

        public void LoadGraph(string fileName)
        {
            _containerCash = Resources.Load<DialogueContainer>(fileName);

            if (!_containerCash)
            {
                EditorUtility.DisplayDialog("File not found",
                    $"Target dialogue <color=yellow>{fileName}</color> doesn't exist", "OK");

                return;
            }

            ClearGraph();
            CreateNodes();
            //ConnectNodes();
        }

        private void ClearGraph()
        {
            Nodes.Find(x => x.EntryPoint).Guid = _containerCash.NodeLinks[0].BaseNodeGuid;
            foreach (var node in Nodes)
            {
                if (node.EntryPoint) return;

                Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
                _targetGraphView.RemoveElement(node);
            }
        }

        private void CreateNodes()
        {
            foreach (var node in _containerCash.DialogueNodeData)
            {
                var tmpNode = _targetGraphView.CreateDialogueNode(node.DialogueText);
                tmpNode.Guid = node.Guid;

                _targetGraphView.AddElement(tmpNode);

                var ports = _containerCash.NodeLinks.Where(x => x.BaseNodeGuid == node.Guid).ToList();
                ports.ForEach(x => _targetGraphView.AddChoicePort(tmpNode, x.PortName));
            }
        }
    }
}