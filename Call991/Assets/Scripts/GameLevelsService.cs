using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AaDialogueGraph;
using Configs;
using ContentDelivery;
using UniRx;

public class GameLevelsService : IDisposable
{
    public class NodeEvents
    {
        public List<EventVisualData> SoundEvents;
        public List<EventVisualData> ObjectEvents;
        public List<EventVisualData> MusicEvents;
        public List<EventVisualData> RtpcEvents;
    }

    private const string ConfigNotSetMsg = "Resources/GameLavels SO not properly set";

    private readonly GameSet _gameSet;
    private readonly OverridenDialogue _overridenDialogue;
    private readonly DialogueLoggerPm _dialogueLogger;

    private readonly List<GameLevelsSo.LevelGroup> _levelGroups;
    private readonly ReactiveProperty<DialogueContainer> _playLevel;
    public readonly ReactiveCommand<List<AaNodeData>> onNext;
    public readonly ReactiveCommand<List<AaNodeData>> findNext;

    private readonly CompositeDisposable _disposables;
    private readonly ReactiveProperty<LevelData> _levelData;

    private DialoguePm _dialoguePm;
    private DialogueContainer _level;
    private AddressableDownloader _addressableDownloader;

    public DialogueContainer PlayLevel => _playLevel.Value;
    public LevelData LevelData => _levelData.Value;
    public Func<Task<List<SlideNodeData>>> OnGetProjectorImages => GetSliderNodes;
    public bool IsNewspaperSkipped => _overridenDialogue.SkipNewspaper;
    public AddressableDownloader AddressableDownloader => _addressableDownloader;

    public DialogueLoggerPm DialogueLogger => _dialogueLogger;

    public GameLevelsService(GameSet gameSet, OverridenDialogue overridenDialogue,
        DialogueLoggerPm dialogueLogger, AddressableDownloader addressableDownloader)
    {
        _disposables = new CompositeDisposable();
        _gameSet = gameSet;
        _overridenDialogue = overridenDialogue;
        _dialogueLogger = dialogueLogger;
        _addressableDownloader = addressableDownloader;
        _levelGroups = gameSet.GameLevels.LevelGroups;
        _playLevel = new ReactiveProperty<DialogueContainer>(GetStartLevel()).AddTo(_disposables);
        _levelData = new ReactiveProperty<LevelData>().AddTo(_disposables);

        onNext = new ReactiveCommand<List<AaNodeData>>().AddTo(_disposables);
        findNext = new ReactiveCommand<List<AaNodeData>>().AddTo(_disposables);
    }

    public void InitDialogue()
    {
        var level = _overridenDialogue.Dialogue != null
            ? _overridenDialogue.Dialogue
            : PlayLevel;

        _level = level;
        _levelData.Value = new LevelData(_level.GetNodesData(), _level.NodeLinks);

        _dialoguePm?.Dispose();
        _dialoguePm = new DialoguePm(new DialoguePm.Ctx
        {
            LevelData = _levelData,
            FindNext = findNext,
            OnNext = onNext,
            DialogueLogger = _dialogueLogger,
        }).AddTo(_disposables);
    }

    private DialogueContainer GetStartLevel()
    {
        if (_levelGroups == null || _levelGroups.Count < 1 || _levelGroups[0].Group == null
            || _levelGroups[0].Group.Count < 1 || _levelGroups[0].Group[0] == null)
            throw new SystemException(ConfigNotSetMsg);

        return _levelGroups[0].Group[0];
    }

    /// <summary>
    /// Returns all levels and ints child sequences
    /// </summary>
    /// <returns></returns>
    public List<DialogueContainer> GetAllLevels()
    {
        if (!CheckConfig())
            throw new SystemException(ConfigNotSetMsg);

        var result = new List<DialogueContainer>();

        foreach (var levelGroup in _levelGroups)
            result.AddRange(levelGroup.Group);

        return result;
    }

    private bool CheckConfig()
    {
        if (_levelGroups == null || _levelGroups.Count < 1) return false;

        foreach (var levelGroup in _levelGroups)
        {
            if (levelGroup?.Group == null || levelGroup.Group.Count < 1) return false;
            if (levelGroup.Group.Any(container => container == null)) return false;
        }

        return true;
    }

    public bool TryGetNextLevel(out DialogueContainer nextLevel, out bool isGameEnd)
    {
        return TryGetNextLevel(_playLevel.Value, out nextLevel, out isGameEnd);
    }

    private bool TryGetNextLevel(DialogueContainer currentLevel, out DialogueContainer nextLevel, out bool isGameEnd)
    {
        nextLevel = null;
        isGameEnd = false;

        if (!CheckConfig())
            throw new SystemException(ConfigNotSetMsg);

        for (var i = 0; i < _levelGroups.Count; i++)
        {
            var group = _levelGroups[i].Group;
            if (!group.Contains(currentLevel)) continue;

            var currentIndex = group.IndexOf(currentLevel);
            if (currentIndex < group.Count - 1)
            {
                nextLevel = group[currentIndex + 1];
                return true;
            }

            if (i >= _levelGroups.Count - 1)
            {
                isGameEnd = true;
                return false;
            }

            nextLevel = _levelGroups[i + 1].Group[0];
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

        return _levelGroups.Select(levelGroup => levelGroup.Group[0]).ToList();
    }

    public List<DialogueContainer> GetLevels()
    {
        return _gameSet.GameLevels.ShowAllLevels ? GetAllLevels() : GetOnlyLevels();
    }

    public string GetLevelId(int index)
    {
        return GetLevels()[index].EntryNodeData.LevelId;
    }

    public void SetLevel(int index)
    {
        var levels = GetLevels();
        _playLevel.Value = levels[index];
    }

    public void SetLevel(DialogueContainer dialogue)
    {
        _playLevel.Value = dialogue;
    }

    /// <summary>
    /// Look into level file, pass all the way in silent mode, grab images from Projector events;
    /// </summary>
    /// <returns></returns>
    private async Task<List<SlideNodeData>> GetSliderNodes()
    {
        var result = new List<SlideNodeData>();

        var entryNodeData = _levelData.Value.GetEntryNode();

        if (!entryNodeData.GrabProjectorImages) return result;

        var newList = new List<AaNodeData> { entryNodeData };
        var data = _dialoguePm.FindNext(newList);

        while (data.Count > 0)
        {
            data = _dialoguePm.FindNext(data);
            var aaSlides = data.OfType<SlideNodeData>().ToList();;

            if (aaSlides.Any()) result.AddRange(aaSlides);

            await Task.Delay(1);
        }

        return result;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    public NodeEvents GetEvents(List<AaNodeData> data)
    {
        var allEvents = new List<EventVisualData>();
        var nodeEvents = new NodeEvents
        {
            SoundEvents = new List<EventVisualData>(),
            ObjectEvents = new List<EventVisualData>(),
            MusicEvents = new List<EventVisualData>(),
            RtpcEvents = new List<EventVisualData>(),
        };

        foreach (var aaData in data.Where(phrase => phrase.EventVisualData.Any()))
            allEvents.AddRange(aaData.EventVisualData);

        foreach (var anEvent in allEvents)
        {
            switch (anEvent.Type)
            {
                case PhraseEventType.Music:
                    nodeEvents.MusicEvents.Add(anEvent);
                    break;
                case PhraseEventType.RTPC:
                    nodeEvents.RtpcEvents.Add(anEvent);
                    break;
                case PhraseEventType.AudioClip:
                    nodeEvents.SoundEvents.Add(anEvent);
                    break;
                case PhraseEventType.Image:
                case PhraseEventType.VideoClip:
                case PhraseEventType.GameObject:
                    nodeEvents.ObjectEvents.Add(anEvent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return nodeEvents;
    }
}