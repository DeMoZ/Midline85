using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AaDialogueGraph;
using Configs;
using Core;
using Data;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class LevelScenePm : IDisposable
{
    public struct Ctx
    {
        public ReactiveCommand<List<AaNodeData>> FindNext;
        public ReactiveCommand<List<AaNodeData>> OnNext;

        public DialogueService DialogueService;
        public GameLevelsService GameLevelsService;
        public MediaService MediaService;
        public ReactiveCommand<GameScenes> OnSwitchScene;
        public ReactiveCommand OnClickMenuButton;
        public ReactiveCommand<(List<RecordData> recordData, bool nextLevelExists)> OnLevelEnd;
        public ObjectEvents ObjectEvents;

        public ContentLoader ContentLoader;
        public List<ChoiceButtonView> buttons;
        public CountDownView CountDown;

        public ReactiveCommand OnAfterEnter;
        public GameSet GameSet;
        public string LevelId;
        
        public ReactiveCommand<bool> OnClickPauseButton;
        public Blocker Blocker;
        public CursorSet CursorSettings;
        public OverridenDialogue OverridenDialogue;
        public ReactiveCommand OnClickNextLevelButton;
    }

    private Ctx _ctx;
    private CompositeDisposable _disposables;
    private PhraseSet _currentPhrase;
    private ReactiveCommand<ChoiceButtonView> _onClickChoiceButton;

    private bool _choiceDone;
    private float _phraseTimer;
    private readonly ReactiveProperty<bool> _isPhraseSkipped = new();
    private readonly ReactiveProperty<bool> _isChoiceDone = new();
    private CancellationTokenSource _tokenSource;

    /// <summary>
    /// Choice button selection with keyboard.
    /// </summary>
    private bool _selectionPlaced;

    public LevelScenePm(Ctx ctx)
    {
        Debug.Log($"[{this}] constructor");

        _ctx = ctx;
        _disposables = new CompositeDisposable();
        _tokenSource = new CancellationTokenSource().AddTo(_disposables);

        _onClickChoiceButton = new ReactiveCommand<ChoiceButtonView>().AddTo(_disposables);
        _onClickChoiceButton.Subscribe(OnClickChoiceButton).AddTo(_disposables);

        _ctx.OnNext.Subscribe(OnDialogue).AddTo(_disposables);
        _ctx.DialogueService.OnSkipPhrase.Subscribe(_ => OnSkipPhrase()).AddTo(_disposables);
        _ctx.OnClickPauseButton.Subscribe(SetPause).AddTo(_disposables);

        _ctx.OnClickMenuButton.Subscribe(_ =>
        {
            _ctx.OnSwitchScene.Execute(GameScenes.Menu);
        }).AddTo(_disposables);
        
        _ctx.OnClickNextLevelButton.Subscribe(_ =>
        {
            _ctx.OnSwitchScene.Execute(GameScenes.Level);
        }).AddTo(_disposables);
        
        _ctx.OnAfterEnter.Subscribe(_ => OnAfterEnter()).AddTo(_disposables);
    }

    private async void OnAfterEnter()
    {
        InitTimer();
        InitButtons();
        _ctx.DialogueService.OnShowLevelUi.Execute();
        _ctx.CursorSettings.EnableCursor(true);
        
        await PrepareAudioManager();
        if (_tokenSource.IsCancellationRequested) return;
        
        ExecuteDialogue();
    }

    private async Task PrepareAudioManager()
    {
        await _ctx.MediaService.AudioManager.LoadBank(_ctx.LevelId);
        if (_tokenSource.IsCancellationRequested) return;

        foreach (var rtpc in _ctx.GameSet.RtpcKeys.WwiseRtpcs)
        {
            _ctx.MediaService.AudioManager.PlayRtpc(rtpc, 0);
        }
    }

    private void SetPause(bool pause)
    {
        Time.timeScale = pause ? 0 : 1;

        if (pause)
        {
            _ctx.MediaService.AudioManager.PausePhrasesAndSfx();
        }
        else
        {
            _ctx.MediaService.AudioManager.ResumePhrasesAndSfx();
            _selectionPlaced = false;
        }
    }

    private void OnSkipPhrase()
    {
        Debug.Log($"[{this}] OnSkipPhrase");
        _isPhraseSkipped.Value = true;
    }

    private void ExecuteDialogue()
    {
        Debug.Log($"[{this}] _ctx.FindNext.Execute with no data");
        _ctx.FindNext.Execute(new List<AaNodeData>());
    }

    private List<AaNodeData> _next;
    private List<ChoiceNodeData> _choices;
    private ChoiceNodeData _choice;

    private async void OnDialogue(List<AaNodeData> data)
    {
        _isPhraseSkipped.Value = false;
        _isChoiceDone.Value = false;

        Debug.Log($"[{this}] Received new nodes {data.Count}");

        _next = new List<AaNodeData>();
        _choice = null;

        var phrases = data.OfType<PhraseNodeData>().ToList();
        var imagePhrases = data.OfType<ImagePhraseNodeData>().ToList();
        var ends = data.OfType<EndNodeData>().ToList();
        var events = data.OfType<EventNodeData>().ToList();
        var newspapers = data.OfType<NewspaperNodeData>().ToList();
        _choices = data.OfType<ChoiceNodeData>().ToList();
        var observables = new IObservable<Unit>[] { };

        GetEvents(out var soundEvents, out var objectEvents, out var musicEvents,
            out var rtpcEvents, phrases, imagePhrases, ends, events, newspapers);

        var content = new Dictionary<string, object>();

        // load content with cancellation token. Return on Cancel.
        if (await LoadContent(content, objectEvents, phrases, imagePhrases, newspapers)) return;

        if (objectEvents.Count > 0)
        {
            foreach (var dialogueEvent in objectEvents)
            {
                var routine = Observable.FromCoroutine(() => RunDialogueEvent(dialogueEvent, content));
                observables = observables.Concat(new[] { routine }).ToArray();
            }
        }

        if (ends.Any())
        {
            var end = ends.First();
            RunEndNode(end);
        }

        foreach (var musicData in musicEvents)
        {
            var routine = Observable.FromCoroutine(() => RunMusic(musicData));
            observables = observables.Concat(new[] { routine }).ToArray();
        }
        
        foreach (var soundData in soundEvents)
        {
            var routine = Observable.FromCoroutine(() => RunSound(soundData));
            observables = observables.Concat(new[] { routine }).ToArray();
        }

        foreach (var rtpcData in rtpcEvents)
        {
            var routine = Observable.FromCoroutine(() => RunRtpc(rtpcData));
            observables = observables.Concat(new[] { routine }).ToArray();
        }

        foreach (var phraseData in phrases)
        {
            var phrase = content[$"p_{phraseData.Guid}"] as Phrase;
            var routine = Observable.FromCoroutine(() => RunPhrase(phraseData, phrase));
            observables = observables.Concat(new[] { routine }).ToArray();
        }
        
        foreach (var imagePhraseData in imagePhrases)
        {
            var phrase = content[$"p_{imagePhraseData.Guid}"] as Phrase;
            var sprite = content[imagePhraseData.Guid] as Sprite;
            var routine = Observable.FromCoroutine(() => RunImagePhrase(imagePhraseData, phrase, sprite));
            observables = observables.Concat(new[] { routine }).ToArray();
        }

        foreach (var eventData in events)
        {
            var routine = Observable.FromCoroutine(() => RunEventNode(eventData));
            observables = observables.Concat(new[] { routine }).ToArray();
        }

        if (_choices.Any())
        {
            var routine = Observable.FromCoroutine(() => RunChoices(_choices));
            observables = observables.Concat(new[] { routine }).ToArray();
        }

        if (newspapers.Any() && !_ctx.OverridenDialogue.SkipNewspaper)
        {
            var newspaper = newspapers.First();
            var routine = Observable.FromCoroutine(() => RunNewspaperNode(content[newspaper.Guid] as Sprite));
            observables = observables.Concat(new[] { routine }).ToArray();
        }

        Observable.WhenAll(observables)
            .Subscribe(_ =>
            {
                Debug.Log($"[{this}] All coroutines completed");
                _next.AddRange(phrases);
                _next.AddRange(imagePhrases);
                _next.AddRange(events);
                _next.AddRange(newspapers);
                _next.Add(_choice);
                if (_next.Any())
                {
                    _ctx.FindNext?.Execute(_next);
                }

                ResourcesLoader.UnloadUnused(); // todo debatable
            }).AddTo(_disposables);
    }

    private void GetEvents(out List<EventVisualData> soundEvents, out List<EventVisualData> objectEvents,
        out List<EventVisualData> musicEvents, out List<EventVisualData> rtpcEvents,
        IEnumerable<PhraseNodeData> phrases, IEnumerable<ImagePhraseNodeData> imagePhrases, IEnumerable<EndNodeData> ends, 
        IEnumerable<EventNodeData> events, IEnumerable<NewspaperNodeData> newspapers)
    {
        var allEvents = new List<EventVisualData>();
        soundEvents = new List<EventVisualData>();
        objectEvents = new List<EventVisualData>();
        musicEvents = new List<EventVisualData>();
        rtpcEvents = new List<EventVisualData>();

        foreach (var phrase in phrases.Where(phrase => phrase.EventVisualData.Any()))
            allEvents.AddRange(phrase.EventVisualData);

        foreach (var imagePhrase in imagePhrases.Where(iPhrase => iPhrase.EventVisualData.Any()))
            allEvents.AddRange(imagePhrase.EventVisualData);

        foreach (var end in ends.Where(end => end.EventVisualData.Any()))
            allEvents.AddRange(end.EventVisualData);

        foreach (var evt in events.Where(evt => evt.EventVisualData.Any()))
            allEvents.AddRange(evt.EventVisualData);

        foreach (var newspaper in newspapers.Where(newspaper => newspaper.EventVisualData.Any()))
            allEvents.AddRange(newspaper.EventVisualData);

        foreach (var anEvent in allEvents)
        {
            switch (anEvent.Type)
            {
                case PhraseEventType.Music:
                    musicEvents.Add(anEvent);
                    break;
                case PhraseEventType.RTPC:
                    rtpcEvents.Add(anEvent);
                    break;
                case PhraseEventType.AudioClip:
                    soundEvents.Add(anEvent);
                    break;
                case PhraseEventType.Image:
                case PhraseEventType.VideoClip:
                case PhraseEventType.GameObject:
                    objectEvents.Add(anEvent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private async Task<bool> LoadContent(Dictionary<string, object> content, List<EventVisualData> objectEvents,
        List<PhraseNodeData> phrases, List<ImagePhraseNodeData> imagePhrases, List<NewspaperNodeData> newspapers)
    {
        foreach (var eventContent in objectEvents)
        {
            switch (eventContent.Type)
            {
                case PhraseEventType.Image:
                    var sprite = await _ctx.ContentLoader.GetObjectAsync<Sprite>(eventContent.PhraseEvent);
                    content[eventContent.PhraseEvent] = sprite;
                    break;
                case PhraseEventType.VideoClip:
                    var video = await _ctx.ContentLoader.GetObjectAsync<VideoClip>(eventContent.PhraseEvent);
                    content[eventContent.PhraseEvent] = video;
                    break;
                case PhraseEventType.GameObject:
                    var go = await _ctx.ContentLoader.GetObjectAsync<GameObject>(eventContent.PhraseEvent);
                    content[eventContent.PhraseEvent] = go;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_tokenSource.IsCancellationRequested) return true;
        }

        // phrase timing asset
        foreach (var data in phrases)
        {
            var phrase = await _ctx.ContentLoader.GetPhraseAsync(data);
            content[$"p_{data.Guid}"] = phrase;

            if (_tokenSource.IsCancellationRequested) return true;
        }
        
        // image phrase timing asset
        foreach (var data in imagePhrases)
        {
            var phrase = await _ctx.ContentLoader.GetPhraseAsync(data);
            content[$"p_{data.Guid}"] = phrase;

            if (_tokenSource.IsCancellationRequested) return true;
        }

        // image phrase graphics
        foreach (var data in imagePhrases)
        {
            var sprite = await _ctx.ContentLoader.GetSpriteAsync(data.ImagePersonVisualData.Sprite);
            content[data.Guid] = sprite;

            if (_tokenSource.IsCancellationRequested) return true;
        }
        
        // newspaper graphics
        foreach (var data in newspapers)
        {
            var sprite = await _ctx.ContentLoader.GetNewspaperAsync(data);
            content[data.Guid] = sprite;

            if (_tokenSource.IsCancellationRequested) return true;
        }

        // wait for wwise get ready
        while (!_ctx.MediaService.AudioManager.IsReady)
        {
            await Task.Delay(1);
            if (_tokenSource.IsCancellationRequested) return true;
        }

        return false;
    }

    private IEnumerator RunPhrase(PhraseNodeData data, Phrase phrase)
    {
        Debug.Log($"[{this}] RunPhrase {data}");
        var defaultTime = 4f;

        var uiPhrase = new UiPhraseData
        {
            Description = data.PhraseSketchText,
            PersonVisualData = data.PersonVisualData,
            PhraseVisualData = data.PhraseVisualData,
            Phrase = phrase,
        };

        _ctx.DialogueService.OnShowPhrase.Execute(uiPhrase);
        var voice = _ctx.GameSet.VoicesSet.GetSoundByPath(data.PhraseSound);
        var voiceId = _ctx.MediaService.AudioManager.PlayVoice(voice);

        if (voiceId == null)
            Debug.LogError($"NONE sound for phrase {data.PhraseSketchText}");
        else
            Debug.Log($"Sound for phrase {data.PhraseSketchText}");
        
        var time = phrase == null ? defaultTime : phrase.totalTime;
        foreach (var t in Timer(time, _isPhraseSkipped)) yield return t;

        if (voiceId != null) _ctx.MediaService.AudioManager.StopVoice(voiceId.Value);

        _ctx.DialogueService.OnHidePhrase.Execute(uiPhrase);
    }
    private IEnumerator RunImagePhrase(ImagePhraseNodeData data, Phrase phrase, Sprite sprite)
    {
        Debug.Log($"[{this}] RunPhrase {data}");
        var defaultTime = 4f;

        var uiPhrase = new UiImagePhraseData
        {
            Description = data.PhraseSketchText,
            PersonVisualData = data.ImagePersonVisualData,
            PhraseVisualData = data.PhraseVisualData,
            Phrase = phrase,
            Sprite = sprite,
        };

        _ctx.DialogueService.OnShowImagePhrase.Execute(uiPhrase);
        var voice = _ctx.GameSet.VoicesSet.GetSoundByPath(data.PhraseSound);
        var voiceId = _ctx.MediaService.AudioManager.PlayVoice(voice);

        if (voiceId == null)
            Debug.LogError($"NONE sound for phrase {data.PhraseSketchText}");
        else
            Debug.Log($"Sound for phrase {data.PhraseSketchText}");
        
        var time = phrase == null ? defaultTime : phrase.totalTime;
        foreach (var t in Timer(time, _isPhraseSkipped)) yield return t;

        if (voiceId != null) _ctx.MediaService.AudioManager.StopVoice(voiceId.Value);

        _ctx.DialogueService.OnHideImagePhrase.Execute(uiPhrase);
    }

    private IEnumerator RunEventNode(EventNodeData data)
    {
        Debug.Log($"[{this}] RunEventNode {data}");
        yield return null;
    }

    private IEnumerator RunMusic(EventVisualData music)
    {
        yield return new WaitForSeconds(music.Delay);

        var musicName = music.PhraseEvent;

        if (string.IsNullOrEmpty(musicName) || musicName.Equals(AaGraphConstants.None)) yield break;

        if (_ctx.GameSet.MusicSwitchesKeys.TryGetSwitchByName(musicName, out var musicSwitch))
            _ctx.MediaService.AudioManager.PlayMusic(musicSwitch);
        else
            Debug.LogError($"Switch name {musicName} not found in GameSet.MusicSwitchesKeys");
    }
    
    private IEnumerator RunSound(EventVisualData data)
    {
        yield return new WaitForSeconds(data.Delay);

        var soundName = data.PhraseEvent;

        if (string.IsNullOrEmpty(soundName) || soundName.Equals(AaGraphConstants.None)) yield break;
        
        var sfx = _ctx.GameSet.SfxsSet.GetSoundByPath(data.PhraseEvent);
        Debug.Log($"Sound {sfx} for phrase {data.PhraseEvent}");

        var sfxId = _ctx.MediaService.AudioManager.PlaySfx(sfx);

        if (sfxId == null)
            Debug.LogError($"NONE sound for phrase {data.PhraseEvent}");
        else
            Debug.Log($"Sound for phrase {data.PhraseEvent}");
        
        if (sfxId != null) 
            _ctx.MediaService.AudioManager.StopVoice(sfxId.Value);
    }

    private IEnumerator RunRtpc(EventVisualData rtpcData)
    {
        yield return new WaitForSeconds(rtpcData.Delay);

        var rtpcName = rtpcData.PhraseEvent;

        if (string.IsNullOrEmpty(rtpcName) || rtpcName.Equals(AaGraphConstants.None)) yield break;

        if (_ctx.GameSet.RtpcKeys.TryGetRtpcByName(rtpcName, out var rtpc))
            _ctx.MediaService.AudioManager.PlayRtpc(rtpc, (int)rtpcData.Value);
        else
            Debug.LogError($"RTPC name {rtpcName} not found in GameSet.RtpcKeys");
    }

    private IEnumerator RunDialogueEvent(EventVisualData data, Dictionary<string, object> content)
    {
        foreach (var t in Timer(data.Delay, _isPhraseSkipped)) yield return t;
        yield return ProcessEvent(data, content);
    }

    private IEnumerator ProcessEvent(EventVisualData data, Dictionary<string, object> content)
    {
        switch (data.Type)
        {
            case PhraseEventType.Image:
                var sprite = content[data.PhraseEvent] as Sprite;
                if (sprite == null && !data.Stop) break;
                _ctx.MediaService.ImageManager.ShowImage(data, sprite);
                break;
            case PhraseEventType.VideoClip:
                var videoClip = content[data.PhraseEvent] as VideoClip;
                if (videoClip == null && !data.Stop) break;
                _ctx.MediaService.VideoManager.PlayVideo(data, videoClip);
                break;
            case PhraseEventType.GameObject:
                var prefab = content[data.PhraseEvent] as GameObject;
                if (prefab == null) break;
                var eventObject = Object.Instantiate(prefab).GetComponent<AaGraphObjectEvent>();
                eventObject.SetCtx(_ctx.ObjectEvents, _ctx.GameSet);

                yield return eventObject.AwaitInvoke();

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        yield return null;
    }

    private IEnumerator RunChoices(List<ChoiceNodeData> data)
    {
        _ctx.CountDown.Show(_ctx.GameSet.choicesDuration);
        _ctx.MediaService.AudioManager.PlayTimerSfx();

        for (var i = 0; i < data.Count; i++)
        {
            var choice = data[i];
            _ctx.buttons[i].interactable = !choice.IsLocked;
            _ctx.buttons[i].Show(choice.Choice, choice.IsLocked);
        }

        foreach (var t in Timer(_ctx.GameSet.choicesDuration, _isChoiceDone)) yield return t;

        if (!_isChoiceDone.Value)
        {
            AutoChoice(data);
        }

        _ctx.MediaService.AudioManager.StopTimerSfx();

        //_ctx.AudioManager.PlayUiSound(SoundUiTypes.ChoiceButton);
        _ctx.CountDown.Stop(_ctx.GameSet.fastButtonFadeDuration);

        foreach (var t in Timer(_ctx.GameSet.slowButtonFadeDuration)) yield return t;

        foreach (var button in _ctx.buttons)
        {
            button.gameObject.SetActive(false);
        }
    }

    private IEnumerator RunNewspaperNode(Sprite sprite)
    {
        Debug.Log($"[{this}] RunEventNode {sprite}");

        var container = new Container<bool>();
        if (sprite != null)
        {
            _ctx.DialogueService.OnShowNewspaper?.Execute((container, sprite));
        }
        else
        {
            container.Value = true;
        }

        _ctx.CursorSettings.EnableCursor(false);
        _ctx.Blocker.FadeScreenBlocker(false).Forget();
        yield return new WaitForSeconds(_ctx.GameSet.shortFadeTime);
        _ctx.CursorSettings.EnableCursor(true);

        var delay = new WaitForSeconds(0.1f);
        while (!container.Value)
        {
            yield return delay;
        }

        _ctx.CursorSettings.EnableCursor(false);
        _ctx.Blocker.FadeScreenBlocker(true).Forget();
        yield return new WaitForSeconds(_ctx.GameSet.shortFadeTime);

        _ctx.DialogueService.OnShowLevelUi?.Execute();

        _ctx.CursorSettings.EnableCursor(true);
    }

    private void AutoChoice(List<ChoiceNodeData> data)
    {
        Debug.Log($"[{this}] <color=yellow>choice time up!</color> Random choice!");

        var cnt = data.Count;
        var isBlocked = true;
        var index = 0;
        while (isBlocked)
        {
            index = Random.Range(0, cnt);
            isBlocked = data[index].IsLocked;
        }

        _ctx.buttons[index].gameObject.Select();
        _ctx.buttons[index].Press();
        OnClickChoiceButton(_ctx.buttons[index]);
    }

    private void OnClickChoiceButton(ChoiceButtonView buttonView)
    {
        if (_isChoiceDone.Value) return;
        _isChoiceDone.Value = true;

        _choice = _choices[_ctx.buttons.IndexOf(buttonView)];
    }

    private void RunEndNode(EndNodeData data)
    {
        Debug.Log($"[{this}] level end {data}");

        var nextLevelExists = _ctx.GameLevelsService.TryGetNextLevel(out var nextLevel, out var isGameEnd);
        
        Debug.Log($"next level Exists = {nextLevelExists}");
        
        if (nextLevelExists)
        {
            Debug.Log($"next level {nextLevel.EntryNodeData.LevelId}; isGameEnd = {isGameEnd}");
            _ctx.GameLevelsService.SetLevel(nextLevel);
        }
        
        _ctx.Blocker.FadeScreenBlocker(false).Forget();
        _ctx.OnLevelEnd?.Execute((data.Records, nextLevelExists));
    }

    private IEnumerator ObserveTimer(float time, ReactiveProperty<bool> onSkip = null, Action onEnd = null)
    {
        foreach (var t in Timer(time, onSkip, onEnd)) yield return t;
    }


    private IEnumerable Timer(float time, ReactiveProperty<bool> skipSignal = null, Action onSkip = null)
    {
        var timer = 0f;

        while (timer <= time)
        {
            yield return null;
            timer += Time.deltaTime;

            if (skipSignal is { Value: true })
            {
                onSkip?.Invoke();
                yield break;
            }
        }
    }
//------------------------------------------------------------------------------------------------------------------

// private bool IsBlocked(Choice choice)
// {
//     if (choice.ifSelected)
//         return !_ctx.profile.ContainsChoice(choice.requiredChoices);
//
//     return false;
// }

/*TODO This might be needed
 private void CheckForSelectionPlaced()
{
    if (!_selectionPlaced && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                              Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)))
    {
        // set first selection
        var eventSystemSelectGameObject = RandomSelectButton(new List<Choice>(_currentPhrase.choices));

        if (eventSystemSelectGameObject != null)
        {
            var selectIndex = _currentPhrase.choices.IndexOf(eventSystemSelectGameObject);
            _ctx.buttons[selectIndex].gameObject.Select();
        }

        _selectionPlaced = true;
    }
}

private Choice RandomSelectButton(List<Choice> choices)
{
    if (choices.Count == 0)
        return null;

    var rndChoice = choices[Random.Range(0, choices.Count)];

    if (IsBlocked(rndChoice))
    {
        choices.Remove(rndChoice);
        return RandomSelectButton(new List<Choice>(choices));
    }

    return rndChoice;
}
*/

    private void InitTimer()
    {
        _ctx.CountDown.SetCtx(new CountDownView.Ctx
        {
            buttonsAppearDuration = _ctx.GameSet.buttonsAppearDuration,
        });
    }
    
    private void InitButtons()
    {
        foreach (var button in _ctx.buttons)
        {
            button.SetCtx(new ChoiceButtonView.Ctx
            {
                onClickChoiceButton = _onClickChoiceButton,
                buttonsAppearDuration = _ctx.GameSet.buttonsAppearDuration,
                fastButtonFadeDuration = _ctx.GameSet.fastButtonFadeDuration,
                slowButtonFadeDuration = _ctx.GameSet.slowButtonFadeDuration,
            });
        }
    }

    public void Dispose()
    {
        _ctx.MediaService.ImageManager.HideImages();
        _ctx.MediaService.VideoManager.StopPlayers();
        _tokenSource.Cancel();
        _disposables.Dispose();
        _ctx.MediaService.AudioManager.UnLoadBank();
    }

    private static void PrintArray(List<string> array)
    {
        var s = array.Aggregate("", (current, t) => current + ", " + t);
        Debug.Log(s);
    }
}