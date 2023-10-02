using System;
using System.Collections.Generic;

namespace AaDialogueGraph
{
    [Serializable]
    public class ImagePhraseNodeData : AaNodeData
    {
        public string PhraseSketchText;
        public ImagePersonVisualData ImagePersonVisualData;
        public PhraseVisualData PhraseVisualData;

        public string PhraseSound;
        public List<string> Phrases;
    }
    
    [Serializable]
    public class ImagePersonVisualData
    {
        public string Person;
        public PersonImageScreenPlace ScreenPlace;
        public string Sprite;
        
        public bool ShowOnStart;
        public bool FocusOnStart;
        public bool UnfocusOnEnd;
        public bool HideOnEnd;
    }
}