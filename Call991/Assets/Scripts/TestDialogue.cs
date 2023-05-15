using AaDialogueGraph;
using UnityEngine;

[System.Serializable]
public class TestDialogue : MonoBehaviour
{
    [SerializeField] private bool _skipWarning;
    [SerializeField] private bool _skipTitle;
    [SerializeField] private bool _skipNewspaper;
    [SerializeField] private DialogueContainer _dialogue;

    public OverridenDialogue GetDialogue()
    {
        return new OverridenDialogue(_skipWarning, _skipTitle, _skipNewspaper, _dialogue);
    }
}

public class OverridenDialogue
{
    public bool SkipWarning { get; }
    public bool SkipTitle { get; }
    public bool SkipNewspaper { get; }
    public DialogueContainer Dialogue { get; }

    public OverridenDialogue(bool skipWarning, bool skipTitle, bool skipNewspaper, DialogueContainer dialogue)
    {
        SkipWarning = skipWarning;
        SkipTitle = skipTitle;
        SkipNewspaper = skipNewspaper;
        Dialogue = dialogue;
    }
}