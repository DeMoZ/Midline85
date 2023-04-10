using System;
using System.Collections.Generic;
using Configs;
using UnityEngine;

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
        
        public PhraseNodeData()
        {
            NodeType = AaNodeType.PhraseNode;
        }
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
        public PhraseEventType Type;
        public string PhraseEvent;
        public PhraseEventLayer Layer;
        public bool Loop;
        
        /// <summary>
        /// "If need to stop the same event started in different phrase node")]
        /// </summary>
        public bool Stop;
        public float Delay;

        public T GetEventObject<T>() where T : UnityEngine.Object
        {
            return string.IsNullOrEmpty(PhraseEvent) 
                ? null 
                : NodeUtils.GetObjectByPath<T>(PhraseEvent);
        }
        
        // private bool Loop() =>
        //     EventType is PhraseEventTypes.LoopSfx or PhraseEventTypes.LoopVfx;
        
        // private bool Stop() =>
        //     EventType is PhraseEventTypes.LoopSfx or PhraseEventTypes.LoopVfx;
    }
}