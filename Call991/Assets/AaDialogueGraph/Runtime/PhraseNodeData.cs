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
        
        public string PhraseSound;
        public List<string> Phrases;
    }

    [Serializable]
    public class PersonVisualData
    {
        public string Person;
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