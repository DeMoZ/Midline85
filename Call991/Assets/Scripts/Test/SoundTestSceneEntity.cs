using System.Collections;
using System.Collections.Generic;
using Configs;
using I2.Loc;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SoundTestSceneEntity : MonoBehaviour
{
    [SerializeField] private Transform buttonParent = default;
    [SerializeField] private Button buttonPrefab = default;
    [SerializeField] private WwiseAudio audioManager = default;
    [Space(30)] [SerializeField] private WwiseSoundsKeysList wwiseSoundsKeysList = default;

    private List<string> _keys;
    private uint? _lastSoundUint;
    private WaitForSeconds _waitForSeconds = new(2);
    private Coroutine _testRoutine;

    void Start()
    {
        Initialization();
        CreateButtons();

        _testRoutine = StartCoroutine(RunTest());
    }

    private void Initialization()
    {
        _keys = wwiseSoundsKeysList.GetKeys();

        var levelLanguages = new ReactiveProperty<List<string>>(LocalizationManager.GetAllLanguages());
        var profile = new PlayerProfile();

        var wwiseCtx = new WwiseAudio.Ctx
        {
            LevelLanguages = levelLanguages,
            Profile = profile,
        };
        audioManager.SetCtx(wwiseCtx);
        audioManager.CreatePhraseVoiceObject();
    }

    private void CreateButtons()
    {
        foreach (var key in _keys)
        {
            var btn = Instantiate(buttonPrefab, buttonParent);
            var txt = btn.GetComponentInChildren<TMP_Text>();
            txt.text = key;
            btn.onClick.AddListener(() => PlayButtonKey(key));
        }
    }

    private void PlayButtonKey(string key)
    {
        if (_testRoutine != null)
            StopCoroutine(_testRoutine);

        PlayKey(key);
    }

    private void PlayKey(string key)
    {
        if (_lastSoundUint.HasValue)
            audioManager.StopPhrase(_lastSoundUint.Value);

        _lastSoundUint = audioManager.PlayPhrase(key);

        if (_lastSoundUint == null)
            Debug.LogError($"NONE sound for phrase {key}");
        else
            Debug.Log($"play key = {key}");
    }

    private IEnumerator RunTest()
    {
        foreach (var key in _keys)
        {
            PlayKey(key);
            yield return _waitForSeconds;
        }

        Debug.LogWarning($"End test");
    }
}