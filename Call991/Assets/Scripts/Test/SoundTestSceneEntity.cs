using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Configs;
using I2.Loc;
using TMPro;
using UI;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
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
    [SerializeField] private Button startTimerButton = default;
    [SerializeField] private Button stopTimerButton = default;
    [SerializeField] private Button hoverButton = default;
    [SerializeField] private Button clickMenuButton = default;
    [SerializeField] private Button clickLevelButton = default;
    [Space] [SerializeField] private ButtonAudioSettings menuButtonAudioSettings = default;
    [SerializeField] private ButtonAudioSettings levelButtonAudioSettings = default;

    [Space] [SerializeField] private Slider masterVolumeSlider = default;
    [SerializeField] [Range(0, 100)] private float startMasterVolume = 93f;
    [SerializeField] private Slider musicVolumeSlider = default;
    [SerializeField] [Range(0, 100)] private float startMusicVolume = 50f;
    [SerializeField] private Slider voiceVolumeSlider = default;
    [SerializeField] [Range(0, 100)] private float startVoiceVolume = 100f;
    [SerializeField] private Slider sfxVolumeSlider = default;
    [SerializeField] [Range(0, 100)] private float startSfxVolume = 87;

    [SerializeField] private Transform musicButtonsParent = default;
    [SerializeField] private Transform voiceButtonsParent = default;
    [SerializeField] private Transform sfxButtonsParent = default;

    [SerializeField] private TestMusicButtonWithSlider musicButtonPrefab = default;
    [SerializeField] private Button voiceButtonPrefab = default;

    [SerializeField] private WwiseAudio wwisePrefab = default;

    [Space(30)] [SerializeField] private WwiseMusicSwitchesList wwiseMusicKeysList = default;

    [Space(30)] [SerializeField] private WwiseSoundsList wwiseVoicesKeysList = default;
    [Space(30)] [SerializeField] private WwiseSoundsList wwiseSfxKeysList = default;

    private List<string> _musicKeys;
    private List<string> _voicesKeys;
    private List<string> _sfxsKeys;
    private uint? _lastVoiceUint;
    private uint? _lastSfxUint;
    private WaitForSeconds _waitForSeconds = new(1);
    private Coroutine _testRoutine;
    private List<TestMusicButtonWithSlider> _musicButtons = new();
    private List<Button> _voiceButtons = new();
    private List<Button> _sfxButtons = new();
    private int _currentIndex;
    private WwiseAudio _audioManager;
    private CancellationTokenSource _tokenSource;
    private PlayerProfile _profile;

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
        _sfxsKeys = wwiseSfxKeysList.GetKeys();

        var onSwitchScene = new ReactiveCommand<GameScenes>();
        var levelLanguages = new ReactiveProperty<List<string>>(LocalizationManager.GetAllLanguages());
        _profile = new PlayerProfile();

        var wwiseCtx = new WwiseAudio.Ctx
        {
            LevelLanguages = levelLanguages,
            Profile = _profile,
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
            var topBtn = Instantiate(voiceButtonPrefab, topParent);
            topBtn.GetComponentInChildren<TMP_Text>().text = language;
            topBtn.onClick.AddListener(() => _profile.AudioLanguageChanged.Value = language);
        }

        startButton.onClick.AddListener(StartVoiceRoutine);
        stopButton.onClick.AddListener(StopVoiceRoutine);
        pauseButton.onClick.AddListener(() => _audioManager.PausePhrasesAndSfx());
        resumeButton.onClick.AddListener(() => _audioManager.ResumePhrasesAndSfx());
        startTimerButton.onClick.AddListener(() => _audioManager.PlayTimerSfx());
        stopTimerButton.onClick.AddListener(() => _audioManager.StopTimerSfx());
        hoverButton.onClick.AddListener(() => menuButtonAudioSettings.PlayHoverSound());
        clickMenuButton.onClick.AddListener(() => menuButtonAudioSettings.PlayClickSound());
        clickLevelButton.onClick.AddListener(() => levelButtonAudioSettings.PlayClickSound());

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        voiceVolumeSlider.onValueChanged.AddListener(SetVoiceVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        masterVolumeSlider.value = startMasterVolume;
        musicVolumeSlider.value = startMusicVolume;
        voiceVolumeSlider.value = startVoiceVolume;
        sfxVolumeSlider.value = startSfxVolume;
    }

    private void SetMasterVolume(float volume)
    {
        _profile.OnVolumeSet.Execute((AudioSourceType.Master, volume));
    }

    private void SetMusicVolume(float volume)
    {
        _profile.OnVolumeSet.Execute((AudioSourceType.Music, volume));
    }

    private void SetVoiceVolume(float volume)
    {
        _profile.OnVolumeSet.Execute((AudioSourceType.Voice, volume));
    }

    private void SetSfxVolume(float volume)
    {
        _profile.OnVolumeSet.Execute((AudioSourceType.Sfx, volume));
    }

    private void StartVoiceRoutine()
    {
        StopVoiceRoutine();
        _testRoutine = StartCoroutine(RunTest());
    }

    private void StopVoiceRoutine()
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
            var btn = Instantiate(musicButtonPrefab, musicButtonsParent);
            var txt = btn.Button.GetComponentInChildren<TMP_Text>();
            txt.text = key;
            btn.Button.onClick.AddListener(() => PlayMusicButtonKey(key));
            btn.Slider.onValueChanged.AddListener(value => SetMusicRtpc(key, value));

            _musicButtons.Add(btn);
        }

        for (var i = 0; i < _voicesKeys.Count; i++)
        {
            var key = _voicesKeys[i];
            var btn = Instantiate(voiceButtonPrefab, voiceButtonsParent);
            var txt = btn.GetComponentInChildren<TMP_Text>();
            txt.text = key;
            btn.onClick.AddListener(() => PlayVoiceButtonKey(key));

            _voiceButtons.Add(btn);
        }

        for (var i = 0; i < _sfxsKeys.Count; i++)
        {
            var key = _sfxsKeys[i];
            var btn = Instantiate(voiceButtonPrefab, sfxButtonsParent);
            var txt = btn.GetComponentInChildren<TMP_Text>();
            txt.text = key;
            btn.onClick.AddListener(() => PlaySfxButtonKey(key));

            _sfxButtons.Add(btn);
        }
    }

    private void PlayMusicButtonKey(string key)
    {
        Debug.Log($"Play Music pressed {key}");
        //_audioManager.PlayMusic(key); // переключает One/Two

        if (wwiseMusicKeysList.TryGetSwitchByName(key, out var sw))
            _audioManager.PlayMusic(sw);
        else
            Debug.LogError($"Switch not found for key {key} in keys list");
    }

    private void SetMusicRtpc(string key, float value)
    {
        _audioManager.PlayRtpc(key, (int)value);
    }

    private void PlayVoiceButtonKey(string key)
    {
        StopVoiceRoutine();
        PlayVoiceKey(key);
    }

    private void PlaySfxButtonKey(string key)
    {
        // StopSfxRoutine();
        PlaySfxKey(key);
    }

    private void PlayVoiceKey(string key)
    {
        if (_lastVoiceUint.HasValue)
            _audioManager.StopVoice(_lastVoiceUint.Value);

        if (_audioManager.TryPlayVoice(key, out var voiceId))
        {
            _lastVoiceUint = voiceId;
            Debug.Log($"play voice key = {key}");
        }
        else
            Debug.LogError($"NONE voice for phrase {key}");


        SetButtonColor(_voiceButtons[_currentIndex], _lastVoiceUint == null ? ButtonColor.Red : ButtonColor.Green);
    }

    private void PlaySfxKey(string key)
    {
        if (_lastSfxUint.HasValue)
            _audioManager.StopSfx(_lastSfxUint.Value);

        if (_audioManager.TryPlaySfx(key, out var sfxId))
        {
            _lastSfxUint = sfxId;
            Debug.Log($"play sfx key = {key}");
        }
        else
            Debug.LogError($"NONE sfx for phrase {key}");
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
            PlayVoiceKey(key);
            yield return _waitForSeconds;
        }

        Debug.LogWarning($"End test");
    }

    private void OnDestroy()
    {
        _tokenSource.Cancel();
    }
}