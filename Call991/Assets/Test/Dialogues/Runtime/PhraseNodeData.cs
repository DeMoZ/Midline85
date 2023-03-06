using System;
using System.Collections.Generic;
using UnityEngine;

namespace Test.Dialogues
{
    [Serializable]
    public class PhraseNodeData
    {
        public string Guid;
        public string DialogueText;
        public Vector2 Position;
        public Vector2 Size;
        public PersonVisualData PersonVisualData;
        public PhraseVisualData PhraseVisualData;
        public List<Phrase> Phrases;
    }

    [Serializable]
    public class PersonVisualData
    {
        public Person Person;
        public ScreenPlace ScreenPlace;
        public OnPhraseEnd OnPhraseEnd;
    }

    [Serializable]
    public class PhraseVisualData
    {
        public TextAppear TextAppear;
        public OnPhraseEnd OnPhraseEnd;
    }
}