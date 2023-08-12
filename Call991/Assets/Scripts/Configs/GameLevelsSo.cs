using System;
using System.Collections.Generic;
using AaDialogueGraph;
using UnityEngine;

[CreateAssetMenu]
public class GameLevelsSo : ScriptableObject
{
    [SerializeField] private DialogueContainer startButtonLevel;
    [SerializeField] private List<LevelGroup> levelGroups;
    
    [SerializeField] private List<DialogueContainer> levels;
    
    public DialogueContainer StartButtonLevel => startButtonLevel;
    public List<DialogueContainer> Levels => levels;
    public List<LevelGroup> LevelGroups => levelGroups;

    [Serializable]
    public class LevelGroup
    {
        public List<DialogueContainer> Group;
    }
}