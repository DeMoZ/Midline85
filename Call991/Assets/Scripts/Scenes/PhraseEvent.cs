using System;
using Sirenix.OdinInspector;

[Serializable]
public class PhraseEvent
{
    public string eventId;
    public PhraseEventTypes eventType;
    [ShowIf("Loopable")] public bool stop;
    public float delay;

    private bool Loopable() =>
        eventType is PhraseEventTypes.SoundLoopSfx or PhraseEventTypes.VideoLoop;
}