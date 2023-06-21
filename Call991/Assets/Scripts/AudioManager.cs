using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AaDialogueGraph;
using Configs;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using Event = AK.Wwise.Event;

public class AudioManager : MonoBehaviour
{
    public struct Ctx
    {
        public GameSet GameSet;
        public AudioMixer audioMixer;
        public PlayerProfile playerProfile;
        public string musicPath;
    }

    [SerializeField] private AudioSource musicAudioSource = default;
    [SerializeField] private AudioSource uiAudioSource = default;
    [SerializeField] private AudioSource timerAudioSource = default;
    [SerializeField] private TempAudioSource tempSoundSourcePrefab = default;
    [SerializeField] private LoopAudioSource loopSoundSourcePrefab = default;
    [SerializeField] private ButtonAudioSettings menuButtonAudioSettings = default;
    private AudioSource _phraseAudioSource;

    private string _languagePath;
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
        
        uiAudioSource.volume = _ctx.playerProfile.UiVolume;
        timerAudioSource.volume = _ctx.playerProfile.TimerVolume;
        musicAudioSource.volume = _ctx.playerProfile.MusicVolume;

        _ctx.playerProfile.onVolumeSet.Subscribe(OnVolumeChanged).AddTo(_disposables);
    }

    private void PlayUiSound(Event obj)
    {
        throw new NotImplementedException();
    }

    private void OnDestroy()
    {
        menuButtonAudioSettings.OnHover -= PlayUiSound;
        _tokenSource?.Cancel();
        _disposables?.Dispose();
    }

    public async Task PlayMusic(string key, int index = 0, bool merge = false)
    {
        List<string> musics;

        if (!_ctx.GameSet.musics.TryGetValue(key, out musics))
        {
            Debug.LogError($"[{this}] Music key not found: {key}:{index}");
            return;
        }

        if (index < 0 && index >= musics.Count)
        {
            Debug.LogError($"[{this}] Music index not found: {key}:{index}");
            return;
        }

        var fileName = musics[index];
        var filePath = Path.Combine(_ctx.musicPath, fileName);

        AudioClip musicClip;
        try
        {
            musicClip = await ResourcesLoader.LoadAsync<AudioClip>(filePath);
            if (_tokenSource.IsCancellationRequested) return;
        }
        catch (Exception e)
        {
            Debug.LogError($"[{this}] Music index not found: {key}:{index}\n{e}");
            return;
        }

        if (_currentMusicClip == null || _currentMusicClip != musicClip)
        {
            if (merge)
                MergeMusics();
            else
                PlayMusicFile(musicAudioSource, musicClip, true);

            _currentMusicClip = musicClip;
        }
    }

    private void MergeMusics()
    {
        throw new NotImplementedException();
    }

    private void PlayMusicFile(AudioSource audioSource, AudioClip audioClip, bool loop)
    {
        if (audioClip == null) return;

        Debug.Log($"[{this}] play phrase audio clip {audioClip}");
        audioSource.clip = audioClip;
        audioSource.Play();
        audioSource.loop = loop;
    }

    private void OnVolumeChanged((AudioSourceType source, float volume) value)
    {
        switch (value.source)
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
        }
    }

    public void PlayUiSound(SoundUiTypes type, bool loop = false)
    {
        switch (type)
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
        }
    }

    private void PlayUiSound(AudioClip clip)
    {
        PlayMusicFile(uiAudioSource, clip, false);
    }

    public void StopTimer()
    {
        timerAudioSource.Stop();
        timerAudioSource.clip = null;
    }

    private void PlaySfx(AudioClip audioClip)
    {
        var source = Instantiate<TempAudioSource>(tempSoundSourcePrefab);
        source.PlayAndDestroy(audioClip, () => { _sfxs.Remove(source); });

        _sfxs ??= new List<TempAudioSource>();
        _sfxs.Add(source);
    }

    private void PlayLoopSfx(AudioClip audioClip, bool stop)
    {
        var audioName = audioClip.name;

        _loopSounds ??= new Dictionary<string, LoopAudioSource>();
        _loopSounds.TryGetValue(audioName, out var source);

        if (source)
        {
            if (stop)
            {
                source.Destroy();
                _loopSounds.Remove(audioName);
            }
        }
        else
        {
            if (stop) return;

            source = Instantiate<LoopAudioSource>(loopSoundSourcePrefab);
            source.Play(audioClip);
            _loopSounds.Add(audioName, source);
        }
    }

    public void OnSceneSwitch()
    {
        _loopSounds ??= new Dictionary<string, LoopAudioSource>();

        foreach (var loop in _loopSounds)
        {
            if (loop.Value != null)
                loop.Value.Destroy();
        }

        _loopSounds.Clear();
        timerAudioSource.Stop();
        timerAudioSource.clip = null;
    }

    public void Pause(bool pause)
    {
        if (pause)
            timerAudioSource?.Pause();
        else
            timerAudioSource?.UnPause();

        if (_loopSounds != null)
            foreach (var source in _loopSounds)
            {
                if (source.Value)
                    source.Value.Pause(pause);
            }

        if (_sfxs != null)
            foreach (var source in _sfxs)
            {
                source.Pause(pause);
            }
    }

    private void SetPhraseVolume()
    {
        if (_phraseAudioSource != null)
            _phraseAudioSource.volume = _ctx.playerProfile.PhraseVolume;
    }

    public void PlayEventSound(EventVisualData data, AudioClip audioClip)
    {
        // TODO layers should be added to mixer
        switch (data.Layer)
        {
            case PhraseEventLayer.Effects: // case PhraseEventTypes.LoopSfx:
                if (data.Loop)
                    PlayLoopSfx(audioClip, data.Stop);
                else
                    PlaySfx(audioClip); // case PhraseEventTypes.Sfx:
                break;
            case PhraseEventLayer.Single1: // case PhraseEventTypes.Music:
                PlayMusicFile(musicAudioSource, audioClip, true);
                _currentMusicClip = audioClip;
                break;
            case PhraseEventLayer.Single2: // case PhraseEventTypes.Music: should be second music layer
                PlayMusicFile(musicAudioSource, audioClip, true);
                _currentMusicClip = audioClip;
                break;
            case PhraseEventLayer.Multiple: // on that layer can be several musics at the same time
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}