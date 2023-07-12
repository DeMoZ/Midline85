using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Configs;
using I2.Loc;
using TMPro;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SoundTestSceneEntity : MonoBehaviour
{
    private enum ButtonColor
    {
        White,
        Green,
        Red
    }

    [SerializeField] private Transform topParent = default;
    [SerializeField] private Button startButton = default;
    [SerializeField] private Button stopButton = default;
    [SerializeField] private Button pauseButton = default;
    [SerializeField] private Button resumeButton = default;
    [SerializeField] private Transform musicButtonsParent = default;
    [SerializeField] private Transform voiceButtonsParent = default;
    [SerializeField] private Button buttonPrefab = default;
    [SerializeField] private WwiseAudio wwisePrefab = default;
   
    [Space(30)] [SerializeField] private WwiseMusicSwitchesList wwiseMusicKeysList = default;

    [Space(30)] [SerializeField] private WwiseVoicesList wwiseVoicesKeysList = default;
    
    private List<string> _musicKeys;
    private List<string> _voicesKeys;
    private uint? _lastSoundUint;
    private WaitForSeconds _waitForSeconds = new(1);
    private Coroutine _testRoutine;
    private List<Button> _musicButtons = new();
    private List<Button> _voiceButtons = new();
    private int _currentIndex;
    private WwiseAudio _audioManager;
    private CancellationTokenSource _tokenSource;

    async void Start()
    {
        _tokenSource = new CancellationTokenSource();
        
        await Initialization();
        if (_tokenSource.IsCancellationRequested) return;
        
        CreateButtons();

        //_testRoutine = StartCoroutine(RunTest());
    }

    private async Task Initialization()
    {
        var cursorSettings = Resources.Load<CursorSet>("CursorSet");
        cursorSettings.ApplyCursor(CursorType.Normal);

        _musicKeys = wwiseMusicKeysList.GetKeys();
        _voicesKeys = wwiseVoicesKeysList.GetKeys();
    
        var onSwitchScene = new ReactiveCommand<GameScenes>();
        var levelLanguages = new ReactiveProperty<List<string>>(LocalizationManager.GetAllLanguages());
        var profile = new PlayerProfile();

        var wwiseCtx = new WwiseAudio.Ctx
        {
            LevelLanguages = levelLanguages,
            Profile = profile,
            OnSwitchScene = onSwitchScene,
        };

        _audioManager = Instantiate(wwisePrefab, transform);
        _audioManager.SetCtx(wwiseCtx);
        await _audioManager.Initialize();
        if (_tokenSource.IsCancellationRequested) return;

        if (wwiseMusicKeysList == null)
            Debug.LogWarning($"voices list \"wwiseMusicKeysList\" is not set in inspector");

        if (wwiseVoicesKeysList == null)
            Debug.LogWarning($"voices list \"wwiseVoicesKeysList\" is not set in inspector");

        var bank = wwiseVoicesKeysList.Path.Split("/")[0];
        Debug.Log($"load bank {bank}");
        await _audioManager.LoadBank(bank);
        Debug.Log($"bank {bank} loaded");
        if (_tokenSource.IsCancellationRequested) return;

        foreach (var language in levelLanguages.Value)
        {
            var topBtn = Instantiate(buttonPrefab, topParent);
            topBtn.GetComponentInChildren<TMP_Text>().text = language;
            topBtn.onClick.AddListener(() => profile.AudioLanguageChanged.Value = language);
        }

        startButton.onClick.AddListener(StartRoutine);
        stopButton.onClick.AddListener(StopRoutine);
        pauseButton.onClick.AddListener(()=>_audioManager.PausePhrasesAndSfx());
        resumeButton.onClick.AddListener(()=>_audioManager.ResumePhrasesAndSfx());
    }

    private void StartRoutine()
    {
        StopRoutine();
        _testRoutine = StartCoroutine(RunTest());
    }

    private void StopRoutine()
    {
        if (_testRoutine != null)
        {
            StopCoroutine(_testRoutine);
            _testRoutine = null;
            Debug.LogWarning("Test stopped");
        }
    }

    private void CreateButtons()
    {
        for (var i = 0; i < _musicKeys.Count; i++)
        {
            var key = _musicKeys[i];
            var btn = Instantiate(buttonPrefab, musicButtonsParent);
            var txt = btn.GetComponentInChildren<TMP_Text>();
            txt.text = key;
            btn.onClick.AddListener(() => PlayMusicButtonKey(key));

            _musicButtons.Add(btn);
        }
        
        for (var i = 0; i < _voicesKeys.Count; i++)
        {
            var key = _voicesKeys[i];
            var btn = Instantiate(buttonPrefab, voiceButtonsParent);
            var txt = btn.GetComponentInChildren<TMP_Text>();
            txt.text = key;
            btn.onClick.AddListener(() => PlayVoiceButtonKey(key));

            _voiceButtons.Add(btn);
        }
    }

    private void PlayMusicButtonKey(string key)
    {
        Debug.Log($"Play Music pressed {key}"); 
        //_audioManager.PlayMusic(key); // переключает One/Two

       if( wwiseMusicKeysList.TryGetSwitchByName(key, out var sw)) 
           _audioManager.PlayMusic(sw);
       else
           Debug.LogError($"Switch not found for key {key} in keys list"); 
    }

    private void PlayVoiceButtonKey(string key)
    {
        StopRoutine();
        PlayKey(key);
    }

    private void PlayKey(string key)
    {
        if (_lastSoundUint.HasValue)
            _audioManager.StopVoice(_lastSoundUint.Value);

        _lastSoundUint = _audioManager.PlayVoice(key);

        if (_lastSoundUint == null)
            Debug.LogError($"NONE sound for phrase {key}");
        else
            Debug.Log($"play key = {key}");

        SetButtonColor(_voiceButtons[_currentIndex], _lastSoundUint == null ? ButtonColor.Red : ButtonColor.Green);
    }

    private void SetButtonColor(Button btn, ButtonColor btnColor)
    {
        var color = btnColor switch
        {
            ButtonColor.Green => Color.green,
            ButtonColor.Red => Color.red,
            _ => Color.white
        };

        var colors = btn.colors;
        colors.normalColor = color;
        btn.colors = colors;
    }

    private IEnumerator RunTest()
    {
        foreach (var button in _voiceButtons)
            SetButtonColor(button, ButtonColor.White);

        Debug.LogWarning("Test started");
        for (var i = 0; i < _voicesKeys.Count; i++)
        {
            while (!_audioManager.IsReady) 
                yield return null;
            
            _currentIndex = i;
            var key = _voicesKeys[i];
            PlayKey(key);
            yield return _waitForSeconds;
        }

        Debug.LogWarning($"End test");
    }

    private void OnDestroy()
    {
        _tokenSource.Cancel();
    }
}