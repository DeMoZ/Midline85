using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Configs;
using UI;
using UniRx;
using UnityEngine;
using Event = AK.Wwise.Event;

public class WwiseAudio : MonoBehaviour
{
    private const string DefaultAudioLanguage = "Russian";
    private const string BankMaster = "Master";
    private const string PauseEvent = "Pause";
    private const string ResumeEvent = "Resume";

    public struct Ctx
    {
        public ReactiveProperty<List<string>> LevelLanguages;
        public PlayerProfile Profile;

        public GameSet GameSet;
        public ReactiveCommand<GameScenes> OnSwitchScene;
    }

    [SerializeField] private ButtonAudioSettings menuButtonAudioSettings = default;
    [SerializeField] private ButtonAudioSettings levelButtonAudioSettings = default;

    private Ctx _ctx;
    private GameObject _menuButtonSoundGo;
    private GameObject _phraseGo;
    private GameObject _sfxGo;

    private static bool _isBankLoaded;
    private WaitForSeconds _waitForLoad = new(0.7f);

    private CompositeDisposable _disposables;
    private CancellationTokenSource _tokenSource;

    public bool IsReady => _isBankLoaded;

    public void SetCtx(Ctx ctx)
    {
        _disposables = new CompositeDisposable();
        _tokenSource = new CancellationTokenSource().AddTo(_disposables);

        _menuButtonSoundGo = new GameObject("MenuButtonsSound");
        _menuButtonSoundGo.transform.parent = transform.parent;

        _ctx = ctx;

        menuButtonAudioSettings.OnHover += PlayButtonSound;
        menuButtonAudioSettings.OnClick += PlayButtonSound;

        levelButtonAudioSettings.OnHover += PlayButtonSound;
        levelButtonAudioSettings.OnClick += PlayButtonSound;

        _ctx.Profile.AudioLanguageChanged.Subscribe(OnAudioLanguageChanged).AddTo(_disposables);
        _ctx.Profile.OnVolumeSet.Subscribe(OnVolumeChanged).AddTo(_disposables);
        _ctx.OnSwitchScene.Subscribe(OnSwitchScene).AddTo(_disposables);
    }

    public void CreateLevelVoiceObjects()
    {
        CreatePhraseVoiceObject();
        CreateSfxVoiceObject();
    }
    
    public void DestroyLevelVoiceObjects()
    {
        Destroy(_phraseGo);
        Destroy(_sfxGo);
    }
    
    private void OnSwitchScene(GameScenes scene)
    {
        Debug.LogWarning($"{this} received OnSwitchScene <color=green>{scene}</color>");
    }

    private void OnDestroy()
    {
        menuButtonAudioSettings.OnHover -= PlayButtonSound;
        menuButtonAudioSettings.OnClick -= PlayButtonSound;

        levelButtonAudioSettings.OnHover -= PlayButtonSound;
        levelButtonAudioSettings.OnClick -= PlayButtonSound;

        _tokenSource?.Cancel();
        _disposables?.Dispose();

        AkBankManager.UnloadBank(BankMaster);
    }

    private void OnAudioLanguageChanged(string language)
    {
        StartCoroutine(ChangeLanguageRoutine(language));
        //ChangeLanguageRoutine(language);
    }

    private void OnBankLoaded(uint in_bankid, IntPtr in_inmemorybankptr, AKRESULT in_eloadresult, object in_cookie)
    {
        _isBankLoaded = true;
        Debug.LogWarning($"[{this}] <color=red>!!!</color> Current Wwise language" +
                         $" = {AkSoundEngine.GetCurrentLanguage()} {Time.time - _time}");
    }

    private float _time;
    private AKRESULT _setLanguageResult;

    private IEnumerator ChangeLanguageRoutine(string language)
    {
        _isBankLoaded = false;
        AkSoundEngine.StopAll();
        AkBankManager.UnloadBank(BankMaster);
        //AkBankManager.DoUnloadBanks(); - this doesnt work =(
        yield return _waitForLoad;

        var audioLanguage = language switch
        {
            "English" => "English",
            "Russian" => "Russian",
            //"Spanish" => "Spanish",
            _ => DefaultAudioLanguage
        };

        //AkSoundEngine.SetCurrentLanguage(audioLanguage);

        _setLanguageResult = AKRESULT.AK_Fail;
        while (_setLanguageResult != AKRESULT.AK_Success)
        {
            if (_setLanguageResult != AKRESULT.AK_Busy)
                _setLanguageResult = AkSoundEngine.SetCurrentLanguage(audioLanguage);

            yield return null;
        }

        Debug.Log($"[{this}] Current Wwise language = {AkSoundEngine.GetCurrentLanguage()}");
        yield return _waitForLoad;

        //AkBankManager.LoadBank(BankMaster, true, true);
        _time = Time.time;
        AkBankManager.LoadBankAsync(BankMaster, OnBankLoaded);
        Debug.Log($"[{this}] 0 loaded Current Wwise language. {Time.time - _time}");
        //yield return _waitForLoad;
        Debug.Log($"[{this}] 1 yield Current Wwise language. {Time.time - _time}");
        //_isBankLoaded = true;
    }

    private void OnVolumeChanged((AudioSourceType source, float volume) value)
    {
        /*switch (value.source)
        {
            case AudioSourceType.Phrases:
                SetPhraseVolume();
                break;
            case AudioSourceType.Effects:
                uiAudioSource.volume = value.volume;
                timerAudioSource.volume = value.volume;
                break;
            case AudioSourceType.Music:
                musicAudioSource.volume = value.volume;
                break;
        }*/
    }

    public void StopTimer()
    {
        // timerAudioSource.Stop();
        // timerAudioSource.clip = null;
    }


    public void OnSceneSwitch()
    {
        // _loopSounds ??= new Dictionary<string, LoopAudioSource>();
        //
        // foreach (var loop in _loopSounds)
        // {
        //     if (loop.Value != null)
        //         loop.Value.Destroy();
        // }
        //
        // _loopSounds.Clear();
        // timerAudioSource.Stop();
        // timerAudioSource.clip = null;
    }

    private void SetPhraseVolume()
    {
        // if (_phraseAudioSource != null)
        //     _phraseAudioSource.volume = _ctx.playerProfile.PhraseVolume;
    }

    public uint? PlayPhrase(string sound)
    {
        if (!_isBankLoaded) return null;
        if (PlaySound(sound, _phraseGo, out var soundId)) return soundId;
        return null;
    }
    
    public uint? PlaySfx(string sound)
    {
        if (!_isBankLoaded) return null;
        if (PlaySound(sound, _sfxGo, out var soundId)) return soundId;
        return null;
    }

    private bool PlaySound(string sound, GameObject soundGo, out uint soundId)
    {
        var isNoKey = string.Equals(sound, AaGraphConstants.None) || string.IsNullOrEmpty(sound);

        if (!isNoKey)
        {
            soundId = AkSoundEngine.PostEvent(sound, soundGo);
            return true;
        }

        soundId = new uint();
        return false;
    }

    public void StopPhrase(uint voiceId) => 
        AkSoundEngine.StopPlayingID(voiceId);

    public void PausePhrasesAndSfx() => 
        AkSoundEngine.PostEvent(PauseEvent, _phraseGo);

    public void ResumePhrasesAndSfx() => 
        AkSoundEngine.PostEvent(ResumeEvent, _phraseGo);

    private void CreatePhraseVoiceObject()
    {
        _phraseGo = new GameObject("PhraseVoiceGO");
        _phraseGo.transform.parent = gameObject.transform;
    }
    
    private void CreateSfxVoiceObject()
    {
        _phraseGo = new GameObject("SfxVoiceGO");
        _phraseGo.transform.parent = gameObject.transform;
    }

    private void PlayButtonSound(Event wwiseEvent)
    {
        if (_setLanguageResult == AKRESULT.AK_Busy) 
            Debug.LogWarning($"[{this}] Very busy");

        if(IsReady)
            wwiseEvent.Post(_menuButtonSoundGo);
    }
    
    #region Rabish
    /*public void _PausePhrases()
    {
        foreach (var voiceId in _playingVoices)
            AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Pause, voiceId);
    }*/

    /*public void _UnPausePhrases()
    {
        foreach (var voiceId in _playingVoices)
            AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Resume, voiceId);
    }*/
    #endregion
}