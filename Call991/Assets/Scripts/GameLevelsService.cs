using System;
using System.Collections.Generic;
using System.Linq;
using AaDialogueGraph;
using Configs;
using UniRx;

public class GameLevelsService
{
    private const string ConfigNotSetMsg = "Resources/GameLavels SO not properly set";

    private readonly GameSet _gameSet;
    private readonly List<GameLevelsSo.LevelGroup> levelGroups;
    private readonly ReactiveProperty<DialogueContainer> playLevel;

    public DialogueContainer PlayLevel => playLevel.Value;

    public GameLevelsService(GameSet gameSet)
    {
        _gameSet = gameSet;
        levelGroups = gameSet.GameLevels.LevelGroups;
        playLevel = new ReactiveProperty<DialogueContainer>(GetStartLevel());
    }

    public DialogueContainer GetStartLevel()
    {
        if (levelGroups == null || levelGroups.Count < 1 || levelGroups[0].Group == null
            || levelGroups[0].Group.Count < 1 || levelGroups[0].Group[0] == null)
            throw new SystemException(ConfigNotSetMsg);

        return levelGroups[0].Group[0];
    }

    /// <summary>
    /// Returns all levels and ints child sequences
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SystemException"></exception>
    private List<DialogueContainer> GetAllLevels()
    {
        if (!CheckConfig())
            throw new SystemException(ConfigNotSetMsg);

        var result = new List<DialogueContainer>();

        foreach (var levelGroup in levelGroups) 
            result.AddRange(levelGroup.Group);

        return result;
    }

    private bool CheckConfig()
    {
        if (levelGroups == null || levelGroups.Count < 1) return false;

        foreach (var levelGroup in levelGroups)
        {
            if (levelGroup?.Group == null || levelGroup.Group.Count < 1) return false;
            if (levelGroup.Group.Any(container => container == null)) return false;
        }

        return true;
    }

    public bool TryGetNextLevel(out DialogueContainer nextLevel, out bool isGameEnd)
    {
        return TryGetNextLevel(playLevel.Value, out nextLevel, out isGameEnd);
    }
    
    public bool TryGetNextLevel(DialogueContainer currentLevel, out DialogueContainer nextLevel, out bool isGameEnd)
    {
        nextLevel = null;
        isGameEnd = false;

        if (!CheckConfig())
            throw new SystemException(ConfigNotSetMsg);

        for (var i = 0; i < levelGroups.Count; i++)
        {
            var group = levelGroups[i].Group;
            if (!group.Contains(currentLevel)) continue;

            var currentIndex = group.IndexOf(currentLevel);
            if (currentIndex < group.Count - 1)
            {
                nextLevel = group[currentIndex + 1];
                return true;
            }

            if (i >= levelGroups.Count - 1)
            {
                isGameEnd = true;
                return false;
            }

            nextLevel = levelGroups[i + 1].Group[0];
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns all levels without its child sequences
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SystemException"></exception>
    private List<DialogueContainer> GetOnlyLevels()
    {
        if (!CheckConfig())
            throw new SystemException(ConfigNotSetMsg);

        return levelGroups.Select(levelGroup => levelGroup.Group[0]).ToList();
    }

    public List<DialogueContainer> GetLevels()
    {
        return _gameSet.GameLevels.ShowAllLevels ? GetAllLevels() : GetOnlyLevels();
    }

    public void SetLevel(int index)
    {
        var levels = GetLevels();
        playLevel.Value = levels[index];
    }
    public void SetLevel(DialogueContainer dialogue)
    {
        playLevel.Value = dialogue;
    }
}