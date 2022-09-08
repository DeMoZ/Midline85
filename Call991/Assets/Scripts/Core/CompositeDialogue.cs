using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CompositeDialogue : ScriptableObject
{
    public List<Dialogues> dialogues;

    public Dialogues Load()
    {
        var newDialogues = ScriptableObject.CreateInstance<Dialogues>();
        newDialogues.phrases = new List<Phrase>();
        foreach (var dialogue in dialogues)
        {
            newDialogues.phrases.AddRange(dialogue.phrases);
        }

        return newDialogues;
    }
}