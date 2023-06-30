using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Configs;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using Event = AK.Wwise.Event;

public class WwiseAudio : MonoBehaviour
{
    private const string DefaultAudioLanguage = "Russian";
    private const string BankMaster = "Master";

    public struct Ctx
    {
        public ReactiveProperty<List<string>> LevelLanguages;
        public PlayerProfile Profile;

        public GameSet GameSet;
        public AudioMixer audioMixer;
    }

    [SerializeField] private ButtonAudioSettings menuButtonAudioSettings = default;
    [SerializeField] private ButtonAudioSettings levelButtonAudioSettings = default;

    private Ctx _ctx;
    private GameObject _menuButtonSoundGo;
    private GameObject _phraseGo;
    private List<uint> _playingVoices;

    private bool _isBankLoaded;
    private WaitForSeconds _waitForLoad = new(0.5f);

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
    }

    private IEnumerator ChangeLanguageRoutine(string language)
    {
        _isBankLoaded = false;
        AkSoundEngine.StopAll();
        AkBankManager.UnloadBank(BankMaster);
        yield return _waitForLoad;

        var audioLanguage = language switch
        {
            "English" => "English",
            "Russian" => "Russian",
            //"Spanish" => "Spanish",
            _ => DefaultAudioLanguage
        };

        AkSoundEngine.SetCurrentLanguage(audioLanguage);
        //AkBankManager.LoadBank(BankMaster, true, true);
        AkBankManager.LoadBank(BankMaster, false, false);
        yield return _waitForLoad;
        _isBankLoaded = true;
        Debug.Log($"[{this}] Current Wwise language = {AkSoundEngine.GetCurrentLanguage()}");
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

    // [Obsolete]
    // public uint? PlayPhrase(List<string> sounds)
    // {
    //     var phraseSound = GetWwiseAudioKey(sounds);
    //     var isNoKey = string.Equals(phraseSound, AaGraphConstants.None) || string.IsNullOrEmpty(phraseSound);
    //
    //     if (!isNoKey)
    //     {
    //         var voiceId = AkSoundEngine.PostEvent(phraseSound, _phraseGo);
    //         _playingVoices.Add(voiceId);
    //         return voiceId;
    //     }
    //
    //     return null;
    // }

    public uint? PlayPhrase(string sound)
    {
        if (!_isBankLoaded) return null;

        var isNoKey = string.Equals(sound, AaGraphConstants.None) || string.IsNullOrEmpty(sound);

        if (!isNoKey)
        {
            var voiceId = AkSoundEngine.PostEvent(sound, _phraseGo);
            _playingVoices.Add(voiceId);
            return voiceId;
        }

        return null;
    }

    public void StopPhrase(uint voiceId)
    {
        AkSoundEngine.StopPlayingID(voiceId);
    }

    public void PausePhrases()
    {
        foreach (var voiceId in _playingVoices)
            AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Pause, voiceId);
    }

    public void UnPausePhrases()
    {
        foreach (var voiceId in _playingVoices)
            AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Resume, voiceId);
    }

    public void CreatePhraseVoiceObject()
    {
        _playingVoices = new();
        _phraseGo = new GameObject("PhraseVoiceGO");
        _phraseGo.transform.parent = gameObject.transform;
    }

    public void ClearPhraseVoiceObject()
    {
        foreach (var voiceId in _playingVoices)
            AkSoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Stop, voiceId);

        _playingVoices.Clear();
        Destroy(_phraseGo);
    }

    // private string GetWwiseAudioKey(List<string> data)
    // {
    //     if (_ctx.LevelLanguages.Value == null || _ctx.LevelLanguages.Value.Count == 0) return null;
    //
    //     var index = _ctx.LevelLanguages.Value.IndexOf(_ctx.Profile.AudioLanguage);
    //
    //     if (index == -1) return null;
    //
    //     return data[index];
    // }

    private void PlayButtonSound(Event wwiseEvent)
    {
        wwiseEvent.Post(_menuButtonSoundGo);
    }
}