using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AaDialogueGraph
{
    [System.Serializable]
    public class DialogueContainer : ScriptableObject
    {
        public EntryNodeData EntryNodeData = new();
        public List<PhraseNodeData> PhraseNodeData = new();
        public List<ImagePhraseNodeData> ImagePhraseNodeData = new();
        public List<ChoiceNodeData> ChoiceNodeData = new();
        public List<ForkNodeData> ForkNodeData = new();
        public List<CountNodeData> CountNodeData = new();
        public List<EndNodeData> EndNodeData = new();
        public List<EventNodeData> EventNodeData = new();
        public List<NewspaperNodeData> NewspaperNodeData = new();
        public List<NodeLinkData> NodeLinks = new();

        public Dictionary<string, AaNodeData> GetNodesData()
        {
            var nodes = new List<AaNodeData>();
            nodes.AddRange(PhraseNodeData);
            nodes.AddRange(ImagePhraseNodeData);
            nodes.AddRange(ChoiceNodeData);
            nodes.AddRange(ForkNodeData);
            nodes.AddRange(CountNodeData);
            nodes.AddRange(EndNodeData);
            nodes.AddRange(EventNodeData);
            nodes.AddRange(NewspaperNodeData);
            nodes.Add(EntryNodeData);

            return nodes.ToDictionary(data => data.Guid);
        }
    }
}