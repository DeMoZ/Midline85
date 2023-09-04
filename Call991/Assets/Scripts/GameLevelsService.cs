using System;
using System.Collections.Generic;
using System.Linq;
using AaDialogueGraph;
using Configs;
using UniRx;

public class GameLevelsService : IDisposable
{
    private const string ConfigNotSetMsg = "Resources/GameLavels SO not properly set";

    private readonly GameSet _gameSet;
    private readonly OverridenDialogue _overridenDialogue;

    private readonly List<GameLevelsSo.LevelGroup> levelGroups;
    private readonly ReactiveProperty<DialogueContainer> playLevel;
    public readonly ReactiveCommand<List<AaNodeData>> onNext;
    public readonly ReactiveCommand<List<AaNodeData>> findNext;

    private CompositeDisposable _disposables;
    private readonly DialoguePm _dialoguePm;

    private DialogueContainer PlayLevel => playLevel.Value;
    public Func<List<string>> OnGetProjectorImages => GetProjectorImages;
    
    public GameLevelsService(GameSet gameSet, OverridenDialogue overridenDialogue, DialogueLoggerPm dialogueLogger)
    {
        _disposables = new CompositeDisposable();
        _gameSet = gameSet;
        _overridenDialogue = overridenDialogue;
        levelGroups = gameSet.GameLevels.LevelGroups;
        playLevel = new ReactiveProperty<DialogueContainer>(GetStartLevel()).AddTo(_disposables);
        
        onNext = new ReactiveCommand<List<AaNodeData>>().AddTo(_disposables);
        findNext = new ReactiveCommand<List<AaNodeData>>().AddTo(_disposables);
        
        _dialoguePm = new DialoguePm(new DialoguePm.Ctx
        {
            LevelData = GetLevelData(),
            FindNext = findNext,
            OnNext = onNext,
            DialogueLogger = dialogueLogger,
        }).AddTo(_disposables);
    }

    public LevelData GetLevelData()
    {
        var level = _overridenDialogue.Dialogue != null
            ? _overridenDialogue.Dialogue
            : PlayLevel;
        
        return new LevelData(level.GetNodesData(), level.NodeLinks);    
    }
    
    private DialogueContainer GetStartLevel()
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
    
    private bool TryGetNextLevel(DialogueContainer currentLevel, out DialogueContainer nextLevel, out bool isGameEnd)
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
    
    private List<string> GetProjectorImages()
    {
        // look into level, pass all the way in silent mode, grab images from Projector events;
        var level = playLevel.Value;
        return new List<string>();
        //var a = level.

    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}