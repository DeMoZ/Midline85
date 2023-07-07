using System;

namespace AaDialogueGraph
{
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
        public float Value;

        public T GetEventObject<T>() where T : UnityEngine.Object
        {
            return string.IsNullOrEmpty(PhraseEvent)
                ? null
                : NodeUtils.GetObjectByPath<T>(PhraseEvent);
        }
    }
}