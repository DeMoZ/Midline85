using System.Collections.Generic;
using UnityEngine;

namespace Test.Dialogues
{
    [System.Serializable]
    public class PhraseNodeData
    {
        public string Guid;
        public string DialogueText;
        public Vector2 Position;
        public Vector2 Size;
        public List<Phrase> Phrases;
    }
}