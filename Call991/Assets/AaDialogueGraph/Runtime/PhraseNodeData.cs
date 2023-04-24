using System;
using System.Collections.Generic;

namespace AaDialogueGraph
{
    [Serializable]
    public class PhraseNodeData : AaNodeData
    {
        public string PhraseSketchText;
        public PersonVisualData PersonVisualData;
        public PhraseVisualData PhraseVisualData;
        public List<EventVisualData> EventVisualData;
        public List<string> PhraseSounds;
        public List<string> Phrases;
    }

    [Serializable]
    public class PersonVisualData
    {
        public Person Person;
        public ScreenPlace ScreenPlace;
        public bool HideOnEnd;
    }

    [Serializable]
    public class PhraseVisualData
    {
        public TextAppear TextAppear;
        public bool HideOnEnd;
    }
}