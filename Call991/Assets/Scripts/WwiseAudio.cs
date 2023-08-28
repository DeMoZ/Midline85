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
    private const float WaitSeconds = 1f;

    [SerializeField] private ButtonAudioSettings menuButtonAudioSettings = default;
    [SerializeField] private ButtonAudioSettings levelButtonAudioSettings = default;

    [Space] [SerializeField] private GameObject voiceGo = default;
    [SerializeField] private GameObject musicGo = default;
    [SerializeField] private GameObject sfxGo = default;

    [Space] [SerializeField] private Wwise.Bank BankMaster = default;

    [Space] [SerializeField] private Wwise.RTPC MasterVolume = default;
    [SerializeField] private Wwise.RTPC VoiceVolume = default;
    [SerializeField] private Wwise.RTPC MusicVolume = default;
    [SerializeField] private Wwise.RTPC SfxVolume = default;

    [Space] [SerializeField] private Wwise.Event SfxDecideTimeStart = default;
    [SerializeField] private Wwise.Event SfxDecideTimeEnd = default;
    [SerializeField] private Wwise.Event PauseEvent = default;
    [SerializeField] private Wwise.Event ResumeEvent = default;
    [SerializeField] private Wwise.Event MusicEvent = default;
    
    private Ctx _ctx;

    private bool _isMasterLoaded;
    private bool _isBankLoaded;
    private bool _isLanguageLoaded;

    private WaitForSeconds _waitForLoad;
    private AKRESULT _setLanguageResult;
    private CompositeDisposable _disposables;
    private CancellationTokenSource _tokenMaster;
    private string _currentBank;

    public bool IsReady => _isMasterLoaded && _isBankLoaded && _isLanguageLoaded;

    public void SetCtx(Ctx ctx)
    {
        _disposables = new CompositeDisposable();
        _tokenMaster = new CancellationTokenSource().AddTo(_disposables);
        _waitForLoad = new WaitForSeconds(WaitSeconds);
        _ctx = ctx;

        menuButtonAudioSettings.OnHover += PlayButtonSound;
        menuButtonAudioSettings.OnClick += PlayButtonSound;

        levelButtonAudioSettings.OnHover += PlayButtonSound;
        levelButtonAudioSettings.OnClick += PlayButtonSound;

        _ctx.Profile.AudioLanguageChanged.Subscribe(OnAudioLanguageChanged).AddTo(_disposables);
        _ctx.Profile.OnVolumeSet.Subscribe(OnVolumeChanged).AddTo(_disposables);
        _ctx.OnSwitchScene.Subscribe(OnSwitchScene).AddTo(_disposables);
    }

    public async Task Initialize()
    {
        await Task.Delay((int)(WaitSeconds * 1000));
        if (_tokenMaster.IsCancellationRequested) return;

        Debug.Log($"[{this}] loading <color=green>bank</color> <color=yellow>{BankMaster}</color>");
        BankMaster.LoadAsync();
        await Task.Delay((int)(WaitSeconds * 1000));
        if (_tokenMaster.IsCancellationRequested) return;

        OnVolumeChanged((AudioSourceType.Master , _ctx.Profile.MasterVolume));
        OnVolumeChanged((AudioSourceType.Voice , _ctx.Profile.VoiceVolume));
        OnVolumeChanged((AudioSourceType.Music , _ctx.Profile.MusicVolume));
        OnVolumeChanged((AudioSourceType.Sfx , _ctx.Profile.SfxVolume));
        
        _isMasterLoaded = true;

        Debug.Log($"[{this}] <color=green>Initialize completed</color> play music {MusicEvent}");
        MusicEvent.Post(musicGo);
    }

    public async Task LoadBank(string levelId)
    {
        while (!_isMasterLoaded)
        {
            await Task.Delay(1);
            if (_tokenMaster.IsCancellationRequested) return;
        }

        await Task.Delay((int)(WaitSeconds * 1000));
        if (_tokenMaster.IsCancellationRequested) return;

        Debug.Log($"[{this}] loading <color=green>bank</color> <color=yellow>{levelId}</color>");
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

        _tokenMaster?.Cancel();
        _disposables?.Dispose();

        BankMaster.Unload();
    }

    private async void OnAudioLanguageChanged(string language)
    {
        _isLanguageLoaded = false;

        if (_currentBank != null)
        {
            AkBankManager.UnloadBank(_currentBank);
            await Task.Delay((int)(WaitSeconds * 1000));
            if (_tokenMaster.IsCancellationRequested) return;
        }

        var audioLanguage = language switch
        {
            "English" => "English",
            "Russian" => "Russian",
            //"Spanish" => "Spanish",
            _ => DefaultAudioLanguage

            //"English" => "English",
            //_ => "English"
        };

        _setLanguageResult = AKRESULT.AK_Fail;
        while (_setLanguageResult != AKRESULT.AK_Success)
        {
            if (_setLanguageResult != AKRESULT.AK_Busy)
                _setLanguageResult = AkSoundEngine.SetCurrentLanguage(audioLanguage);

            await Task.Delay(1);
            if (_tokenMaster.IsCancellationRequested) return;
        }

        Debug.Log($"[{this}] Current Wwise language = <color=yellow>{AkSoundEngine.GetCurrentLanguage()}</color>");

        if (_currentBank != null)
        {
            await LoadBank(_currentBank);
            if (_tokenMaster.IsCancellationRequested) return;
        }

        _isLanguageLoaded = true;
    }

    private void OnVolumeChanged((AudioSourceType source, float volume) value)
    {
        //MusicVolume.SetValue(musicGo, value.volume);
        //MusicVolume.SetGlobalValue(value.volume);
        //AkSoundEngine.SetRTPCValue(MusicVolume.Name, value.volume);
        switch (value.source)
        {
            case AudioSourceType.Master:
                MasterVolume.SetGlobalValue(value.volume);
                break;
            case AudioSourceType.Voice:
                VoiceVolume.SetValue(voiceGo, value.volume);
                break;
            case AudioSourceType.Music:
                MusicVolume.SetValue(musicGo, value.volume);
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

    public bool TryPlayVoice(string sound, out uint? voiceId)
    {
        voiceId = null;
        if (!_isBankLoaded) return false;
        if (TryPlaySound(sound, voiceGo, out var soundId))
        {
            voiceId = soundId;
            return true;
        }
        
        return false;
    }

    public void StopVoice(uint voiceId) =>
        AkSoundEngine.StopPlayingID(voiceId);

    public async void PlayMusic(Wwise.Switch wSwitch)
    {
        while (!_isMasterLoaded)
        {
            await Task.Delay(1);
            if (_tokenMaster.IsCancellationRequested) return;
        }
        if (_tokenMaster.IsCancellationRequested) return;

        Debug.Log($"[{this}] <color=green>PlayMusic</color> switch = <color=yellow>{wSwitch}</color>;");
        wSwitch.SetValue(musicGo);
    }

    public async void PlayMusic(string wSwitch)
    {
        while (!_isMasterLoaded)
        {
            await Task.Delay(1);
            if (_tokenMaster.IsCancellationRequested) return;
        }
        if (_tokenMaster.IsCancellationRequested) return;

        Debug.Log($"[{this}] <color=green>PlayMusic</color> switch = <color=yellow>{wSwitch}</color>;");
        var parts = wSwitch.Split(" / ");
        AkSoundEngine.SetSwitch(parts[0], parts[1], musicGo);
    }

    public void PlayRtpc(Wwise.RTPC rtpc, int value)
    {
        Debug.Log($"[{this}] <color=green>PlayRtpc</color> rtpc = <color=yellow>{rtpc};</color> " +
                  $"value = <color=yellow>{value}</color>;");
        rtpc.SetGlobalValue(value);
    }

    public async void PlayRtpc(string wSwitch, int value)
    {
        while (!_isMasterLoaded)
        {
            await Task.Delay(1);
            if (_tokenMaster.IsCancellationRequested) return;
        }
        if (_tokenMaster.IsCancellationRequested) return;
        
        var parts = wSwitch.Split(" / Music");
        var rtpc = $"{parts[1]}RTPC";
        Debug.Log($"[{this}] <color=green>PlayRtpc</color> RTPC = <color=yellow>{rtpc}</color>;" +
                  $" value = <color=yellow>{value}</color>;");
        
        AkSoundEngine.SetRTPCValue(rtpc, value);
    }
    
    public bool TryPlaySfx(string sound, out uint? sfxId)
    {
        if (TryPlaySound(sound, sfxGo, out var soundId))
        {
            sfxId =  soundId;
            return true;
        }
        
        sfxId = null;
        return false;
    }
    
    public void StopSfx(uint sfx) =>
        AkSoundEngine.StopPlayingID(sfx);

    public void PlayTimerSfx()
    {
        if (!_isBankLoaded) return;
        TryPlaySound(SfxDecideTimeStart.ToString(), sfxGo, out var soundId);
    }

    public void StopTimerSfx()
    {
        if (!_isBankLoaded) return;
        TryPlaySound(SfxDecideTimeEnd.ToString(), sfxGo, out var soundId);
    }

    private bool TryPlaySound(string sound, GameObject soundGo, out uint soundId)
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
        PauseEvent.Post(voiceGo);
        PauseEvent.Post(sfxGo);
    }

    public void ResumePhrasesAndSfx()
    {
        ResumeEvent.Post(voiceGo);
        ResumeEvent.Post(sfxGo);
    }

    private void PlayButtonSound(Wwise.Event wwiseEvent)
    {
        if (_setLanguageResult == AKRESULT.AK_Busy)
            Debug.LogWarning($"[{this}] Very busy");

        if (IsReady)
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

    [Obsolete]
    private IEnumerator ChangeLanguageRoutine(string language)
    {
        _isLanguageLoaded = false;
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

            //"English" => "English",
            //_ => "English"
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

        _isLanguageLoaded = true;
    }

    #endregion
}