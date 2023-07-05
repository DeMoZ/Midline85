using System.Collections;
using System.Collections.Generic;
using Configs;
using I2.Loc;
using TMPro;
using UI;
using UniRx;
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
    [SerializeField] private Transform buttonParent = default;
    [SerializeField] private Button buttonPrefab = default;
    [SerializeField] private WwiseAudio wwisePrefab = default;
    [Space(30)] [SerializeField] private WwiseSoundsKeysList wwiseSoundsKeysList = default;

    private List<string> _keys;
    private uint? _lastSoundUint;
    private WaitForSeconds _waitForSeconds = new(1);
    private Coroutine _testRoutine;
    private List<Button> _buttons = new();
    private int _currentIndex;
    private WwiseAudio _audioManager;

    void Start()
    {
        Initialization();
        CreateButtons();

        _testRoutine = StartCoroutine(RunTest());
    }

    private void Initialization()
    {
        var cursorSettings = Resources.Load<CursorSet>("CursorSet");
        cursorSettings.ApplyCursor(CursorType.Normal);

        _keys = wwiseSoundsKeysList.GetKeys();

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
        _audioManager.CreateLevelVoiceObjects();

        foreach (var language in levelLanguages.Value)
        {
            var topBtn = Instantiate(buttonPrefab, topParent);
            topBtn.GetComponentInChildren<TMP_Text>().text = language;
            topBtn.onClick.AddListener(() => profile.AudioLanguageChanged.Value = language);
        }

        startButton.onClick.AddListener(StartRoutine);
        stopButton.onClick.AddListener(StopRoutine);
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
        for (var i = 0; i < _keys.Count; i++)
        {
            var key = _keys[i];
            var btn = Instantiate(buttonPrefab, buttonParent);
            var txt = btn.GetComponentInChildren<TMP_Text>();
            txt.text = key;
            btn.onClick.AddListener(() => PlayButtonKey(key));

            _buttons.Add(btn);
        }
    }

    private void PlayButtonKey(string key)
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

        SetButtonColor(_buttons[_currentIndex], _lastSoundUint == null ? ButtonColor.Red : ButtonColor.Green);
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
        foreach (var button in _buttons)
            SetButtonColor(button, ButtonColor.White);

        Debug.LogWarning("Test started");
        for (var i = 0; i < _keys.Count; i++)
        {
            while (!_audioManager.IsReady) 
                yield return null;
            
            _currentIndex = i;
            var key = _keys[i];
            PlayKey(key);
            yield return _waitForSeconds;
        }

        Debug.LogWarning($"End test");
    }
}