using System.Collections.Generic;
using UnityEngine;

namespace Test.Dialogues
{
    [System.Serializable]
    public class DialogueContainer : ScriptableObject
    {
        public List<DialogueNodeData> DialogueNodeData = new();
        public List<NodeLinkData> NodeLinks = new();
    }
}