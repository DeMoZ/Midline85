using System.Collections.Generic;
using UnityEngine;

namespace Test.Dialogues
{
    [System.Serializable]
    public class DialogueContainer : ScriptableObject
    {
        public List<string> Languages = new();
        public string EntryGuid;
        public Rect EntryRect;
        public List<PhraseNodeData> PhraseNodeData = new();
        public List<ChoiceNodeData> ChoiceNodeData = new();
        public List<NodeLinkData> NodeLinks = new();
    }
}