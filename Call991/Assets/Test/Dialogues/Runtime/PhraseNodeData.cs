using System;
using System.Collections.Generic;
using Configs;
using UnityEngine;

namespace Test.Dialogues
{
    [Serializable]
    public class PhraseNodeData : AaNodeData
    {
        public string PhraseSketchText;
        
        public PersonVisualData PersonVisualData;
        public PhraseVisualData PhraseVisualData;
        public List<EventVisualData> EventVisualData;
        public List<AudioClip> PhraseSounds;
        public List<Phrase> Phrases;
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

    [Serializable]
    public class EventVisualData
    {
        public PhraseEventSo PhraseEventSo;
        public PhraseEventTypes EventType = PhraseEventTypes.Sfx;

        [Tooltip("If need to stop the same event started in different phrase node")]
        public bool Stop;
        public float Delay;
        
        private bool Loopable() =>
            EventType is PhraseEventTypes.LoopSfx or PhraseEventTypes.LoopVfx;
    }
}