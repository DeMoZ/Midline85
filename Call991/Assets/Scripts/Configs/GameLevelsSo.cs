using System;
using System.Collections.Generic;
using AaDialogueGraph;
using UnityEngine;

[CreateAssetMenu]
public class GameLevelsSo : ScriptableObject
{
    [Tooltip("Show all levels and its sequences in one list")] [Space] [SerializeField]
    private bool showAllLevels;
    [Space] [SerializeField] private List<LevelGroup> levelGroups;

    public List<LevelGroup> LevelGroups => levelGroups;
    public bool ShowAllLevels => showAllLevels;

    [Serializable]
    public class LevelGroup
    {
        public List<DialogueContainer> Group;
    }
}