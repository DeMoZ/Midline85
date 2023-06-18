using System.Collections.Generic;
using System.Threading;
using AaDialogueGraph;
using Configs;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;

public class WwiseAudio : MonoBehaviour
{
    public struct Ctx
    {
        public ReactiveProperty<List<string>> LevelLanguages;
        public PlayerProfile Profile;

        public GameSet GameSet;
        public AudioMixer audioMixer;
        public string musicPath;
    }

    [SerializeField] private ButtonAudioSettings menuButtonAudioSettings = default;

    private Ctx _ctx;
    private GameObject _phraseGo;
    private List<uint> _playingVoices;


    private CompositeDisposable _disposables;
    private CancellationTokenSource _tokenSource;

    public void SetCtx(Ctx ctx)
    {
        _disposables = new CompositeDisposable();
        _tokenSource = new CancellationTokenSource().AddTo(_disposables);

        _ctx = ctx;
        menuButtonAudioSettings.OnHover += PlayUiSound;
        _ctx.Profile.onVolumeSet.Subscribe(OnVolumeChanged).AddTo(_disposables);
    }

    private void OnDestroy()
    {
        menuButtonAudioSettings.OnHover -= PlayUiSound;
        _tokenSource?.Cancel();
        _disposables?.Dispose();
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

    public void PlayUiSound(SoundUiTypes type, bool loop = false)
    {
        /*switch (type)
        {
            case SoundUiTypes.ChoiceButton:
                PlayMusicFile(uiAudioSource, _ctx.GameSet.choiceBtnClip, false);
                break;
            case SoundUiTypes.MenuButton:
                PlayMusicFile(uiAudioSource, _ctx.GameSet.menuBtnClip, false);
                break;
            case SoundUiTypes.Timer:
                PlayMusicFile(timerAudioSource, _ctx.GameSet.timerClip, true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }*/
    }

    private void PlayUiSound(AudioClip clip)
    {
        //PlayMusicFile(uiAudioSource, clip, false);
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

    public uint? PlayPhrase(List<string> sounds)
    {
        var phraseSound = GetWwiseAudioKey(sounds);
        var isNoKey = string.Equals(phraseSound, AaGraphConstants.None) || string.IsNullOrEmpty(phraseSound);

        if (!isNoKey)
        {
            var voiceId = AkSoundEngine.PostEvent(phraseSound, _phraseGo);
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

    private string GetWwiseAudioKey(List<string> data)
    {
        if (_ctx.LevelLanguages.Value == null || _ctx.LevelLanguages.Value.Count == 0) return null;

        var index = _ctx.LevelLanguages.Value.IndexOf(_ctx.Profile.AudioLanguage);

        if (index == -1) return null;

        return data[index];
    }


    //------
    public AK.Wwise.Event someSound;

    void _Start()
    {
        // Play the sound
        someSound.Post(gameObject);
    }

    void _Update()
    {
        // Pause the sound
        if (Input.GetKeyDown(KeyCode.Space))
        {
            someSound.ExecuteAction(_phraseGo, AkActionOnEventType.AkActionOnEventType_Pause, 0,
                AkCurveInterpolation.AkCurveInterpolation_Linear);
        }

        // Resume the sound
        if (Input.GetKeyUp(KeyCode.Space))
        {
            someSound.ExecuteAction(gameObject, AkActionOnEventType.AkActionOnEventType_Resume, 0,
                AkCurveInterpolation.AkCurveInterpolation_Linear);
        }
    }
}