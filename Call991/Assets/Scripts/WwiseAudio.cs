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
    private string _currentMusicPath;
    private AudioClip _currentMusicClip;
    private Dictionary<string, LoopAudioSource> _loopSounds;
    private List<TempAudioSource> _sfxs;

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

    public void Pause(bool pause)
    {
        // if (pause)
        //     timerAudioSource?.Pause();
        // else
        //     timerAudioSource?.UnPause();
        //
        // if (_loopSounds != null)
        //     foreach (var source in _loopSounds)
        //     {
        //         if (source.Value)
        //             source.Value.Pause(pause);
        //     }
        //
        // if (_sfxs != null)
        //     foreach (var source in _sfxs)
        //     {
        //         source.Pause(pause);
        //     }
    }

    private void SetPhraseVolume()
    {
        // if (_phraseAudioSource != null)
        //     _phraseAudioSource.volume = _ctx.playerProfile.PhraseVolume;
    }

    public void PlayEventSound(EventVisualData data, AudioClip audioClip)
    {
        
    }

    public uint? PlayPhrase(List<string> sounds, string phraseSketchText)
    {
        var phraseSound = GetWwiseAudioKey(sounds);
        var isNone = string.Equals(phraseSound, AaGraphConstants.None);
        if (!isNone)
        {
            return AkSoundEngine.PostEvent(phraseSound, gameObject);
        }

        Debug.LogError($"NONE sound for phrase {phraseSketchText}");
        return null;
    }
    
    public void StopPhrase(uint? voiceId)
    {
        if (voiceId.HasValue)
           AkSoundEngine.StopPlayingID(voiceId.Value); 
    }
    
    private string GetWwiseAudioKey(List<string> data)
    {
        if (_ctx.LevelLanguages.Value == null || _ctx.LevelLanguages.Value.Count == 0) return null;

        var index = _ctx.LevelLanguages.Value.IndexOf(_ctx.Profile.AudioLanguage);

        if (index == -1) return null;

        return data[index];
    }
}