using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Configs;
using UI;
using UniRx;
using UnityEngine;
using Wwise = AK.Wwise;

public class WwiseAudio : MonoBehaviour
{
    public struct Ctx
    {
        public ReactiveProperty<List<string>> LevelLanguages;
        public PlayerProfile Profile;

        public GameSet GameSet;
        public ReactiveCommand<GameScenes> OnSwitchScene;
    }
    private const string DefaultAudioLanguage = "Russian";
    private const string BankMaster = "Master";
    
    // Hardcoded events
    private const string PauseEvent = "Pause";
    private const string ResumeEvent = "Resume";

    private const float WaitSeconds = 1f;
    
    [SerializeField] private ButtonAudioSettings menuButtonAudioSettings = default;
    [SerializeField] private ButtonAudioSettings levelButtonAudioSettings = default;
    [Space]
    [SerializeField] private GameObject voiceGo = default;
    [SerializeField] private GameObject musicGo = default;
    [SerializeField] private GameObject sfxGo = default;
    [Space]
    [SerializeField] private Wwise.RTPC MasterVolume = default;
    [SerializeField] private Wwise.RTPC VoiceVolume = default;
    [SerializeField] private Wwise.RTPC MusicVolume = default;
    [SerializeField] private Wwise.RTPC SfxVolume = default;
    [Space]
    [SerializeField] private Wwise.Event SfxDecideTimeStart = default;
    [SerializeField] private Wwise.Event SfxDecideTimeEnd = default;
    [SerializeField] private Wwise.Event MusicEvent = default;


    [SerializeField] private Wwise.RTPC testrtpc;
    
    private Ctx _ctx;

    private static bool _isBankLoaded;
    private WaitForSeconds _waitForLoad;
    private float _time;
    private AKRESULT _setLanguageResult;
    
    private CompositeDisposable _disposables;
    private CancellationTokenSource _tokenSource;
    private string _currentBank;

    public bool IsReady => _isBankLoaded;

    public void SetCtx(Ctx ctx)
    {
        _disposables = new CompositeDisposable();
        _tokenSource = new CancellationTokenSource().AddTo(_disposables);
        _waitForLoad = new WaitForSeconds(WaitSeconds);
        _ctx = ctx;

        menuButtonAudioSettings.OnHover += PlayButtonSound;
        menuButtonAudioSettings.OnClick += PlayButtonSound;

        levelButtonAudioSettings.OnHover += PlayButtonSound;
        levelButtonAudioSettings.OnClick += PlayButtonSound;

        _ctx.Profile.AudioLanguageChanged.Subscribe(OnAudioLanguageChanged).AddTo(_disposables);
        _ctx.Profile.OnVolumeSet.Subscribe(OnVolumeChanged).AddTo(_disposables);
        _ctx.OnSwitchScene.Subscribe(OnSwitchScene).AddTo(_disposables);

        Initialize();
    }

    private async void Initialize()
    {
        await Task.Delay((int)(WaitSeconds * 1000));
        if (_tokenSource.IsCancellationRequested) return;
        
        AkBankManager.LoadBankAsync(BankMaster);
        await Task.Delay((int)(WaitSeconds * 1000));
        if (_tokenSource.IsCancellationRequested) return;
        
        MusicEvent.Post(musicGo);
        _isBankLoaded = true;
        
        // PlayMusic(MusicEventSwitch); // Test
    }
    
    public async void LoadBank(string levelId)
    {
        Debug.LogWarning($"[{this}] loading bank {levelId}");
        _isBankLoaded = false;
        _currentBank = levelId;
        AkBankManager.LoadBankAsync(levelId);
        await Task.Delay((int)(WaitSeconds * 1000));
        _isBankLoaded = true;
    }
    
    public async void UnLoadBank()
    {
        if (string.IsNullOrEmpty(_currentBank)) return;
        
        _isBankLoaded = false;
        AkBankManager.UnloadBank(_currentBank);
        await Task.Delay((int)(WaitSeconds * 1000));
        _isBankLoaded = true;
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
        Debug.Log($"[{this}] Current Wwise language" +
                         $" = <color=green>{AkSoundEngine.GetCurrentLanguage()}</color> {Time.time - _time}");
    }

    private IEnumerator ChangeLanguageRoutine(string language)
    {
        _isBankLoaded = false;
        //AkSoundEngine.StopAll();
        //AkBankManager.UnloadBank(BankMaster);
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

        Debug.Log($"[{this}] Current Wwise language = <color=yellow>{AkSoundEngine.GetCurrentLanguage()}</color>");
        yield return _waitForLoad;

        //AkBankManager.LoadBank(BankMaster, true, true);
        _time = Time.time;
        //AkBankManager.LoadBankAsync(BankMaster, OnBankLoaded); // TODO load level bank
        _isBankLoaded = true; // TODO Remove!!!
    }

    private void OnVolumeChanged((AudioSourceType source, float volume) value)
    {
        switch (value.source)
        {
            case AudioSourceType.Master:
                MasterVolume.SetGlobalValue(value.volume);
                break;
            case AudioSourceType.Voice:
                VoiceVolume.SetValue(voiceGo, value.volume);
                //VoiceVolume.SetGlobalValue(value.volume); // also works
                break;
            case AudioSourceType.Music:
                //MusicVolume.SetValue(musicGo, value.volume);
                Debug.LogWarning($"music {MusicVolume}, value {value.volume}");
                MusicVolume.SetGlobalValue(value.volume);
                break;
            case AudioSourceType.Sfx:
                SfxVolume.SetValue(sfxGo, value.volume);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void OnSceneSwitch()
    {
        
    }
    
    public uint? PlayVoice(string sound)
    {
        if (!_isBankLoaded) return null;
        if (PlaySound(sound, voiceGo, out var soundId)) return soundId;
        return null;
    }
    
    public void StopVoice(uint voiceId) => 
        AkSoundEngine.StopPlayingID(voiceId);

    public void PlayMusic(Wwise.Switch wSwitch)
    {
        Debug.Log($"[{this}] <color=green>PlayMusic</color> switch = <color=yellow>{wSwitch}</color>;");
        wSwitch.SetValue(musicGo);
    }
    
    public void PlayRtpc(Wwise.RTPC rtpc, int value)
    {
        Debug.Log($"[{this}] <color=green>PlayRtpc</color> rtpc = <color=yellow>{rtpc};</color> " +
                  $"value = <color=yellow>{value}</color>;");
        rtpc.SetGlobalValue(value);
    }
    
    public uint? PlaySfx(string sound)
    {
        if (!_isBankLoaded) return null;
        if (PlaySound(sound, sfxGo, out var soundId)) return soundId;
        return null;
    }
    
    public void PlayTimerSfx()
    {
        if (!_isBankLoaded) return;
        PlaySound(SfxDecideTimeStart.ToString(), sfxGo, out var soundId);
    }
    
    public void StopTimerSfx()
    {
        if (!_isBankLoaded) return;
        PlaySound(SfxDecideTimeEnd.ToString(), sfxGo, out var soundId);
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

    public void PausePhrasesAndSfx()
    {
        AkSoundEngine.PostEvent(PauseEvent, voiceGo);
        AkSoundEngine.PostEvent(PauseEvent, sfxGo); // ?
    }

    public void ResumePhrasesAndSfx()
    {
        AkSoundEngine.PostEvent(ResumeEvent, voiceGo);
        AkSoundEngine.PostEvent(ResumeEvent, sfxGo); // ?
    }

    private void PlayButtonSound(Wwise.Event wwiseEvent)
    {
        if (_setLanguageResult == AKRESULT.AK_Busy) 
            Debug.LogWarning($"[{this}] Very busy");

        if(IsReady)
            wwiseEvent.Post(sfxGo);
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