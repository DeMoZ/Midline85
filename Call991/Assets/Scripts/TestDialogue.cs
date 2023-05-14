using AaDialogueGraph;
using UnityEngine;

[System.Serializable]
public class TestDialogue : MonoBehaviour
{
    [SerializeField] private bool _levelOnly;
    [SerializeField] private DialogueContainer _dialogue;

    public OverridenDialogue GetDialogue()
    {
        return new OverridenDialogue(_levelOnly, _dialogue);
    }
}

public class OverridenDialogue
{
    public bool LevelOnly { get; }
    public DialogueContainer Dialogue { get; }

    public OverridenDialogue(bool levelOnly, DialogueContainer dialogue)
    {
        LevelOnly = levelOnly;
        Dialogue = dialogue;
    }
}