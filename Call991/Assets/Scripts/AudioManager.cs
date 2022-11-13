using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Configs;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public struct Ctx
    {
        public GameSet gameSet;
        public ReactiveCommand<Language> onAudioLanguage;
        public AudioMixer audioMixer;
        public string soundPath;
        public string musicPath;
        public string voiceFolder;
        public string levelFolder;
    }

    [SerializeField] private AudioSource musicAudioSource = default;
    [SerializeField] private AudioSource uiAudioSource = default;
    [SerializeField] private AudioSource timerAudioSource = default;
    [SerializeField] private TempAudioSource tempSoundSourcePrefab = default;
    [SerializeField] private LoopAudioSource loopSoundSourcePrefab = default;
    [SerializeField] private ButtonAudioSettings menuButtonAudioSettings = default;

    private string _languagePath;
    private Ctx _ctx;
    private string _currentMusicPath;
    private AudioClip _currentMusicClip;
    private Dictionary<string, LoopAudioSource> _loopSounds;
    private List<TempAudioSource> _sfxs;

    public void SetCtx(Ctx ctx)
    {
        _ctx = ctx;
        menuButtonAudioSettings.OnHover += PlayUiSound;
    }

    private void OnDestroy()
    {
        menuButtonAudioSettings.OnHover -= PlayUiSound;
    }

    public string LanguagePath
    {
        get => _languagePath;
        set => _languagePath = value;
    }

    public async Task PlayMusic(string key, int index = 0, bool merge = false)
    {
        List<string> musics;

        if (!_ctx.gameSet.musics.TryGetValue(key, out musics))
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

    private void PlayUiSound(AudioClip clip)
    {
        PlayMusicFile(uiAudioSource, clip, false);
    }
    
    public void PlayUiSound(SoundUiTypes type, bool loop = false)
    {
        switch (type)
        {
            case SoundUiTypes.ChoiceButton:
                PlayMusicFile(uiAudioSource, _ctx.gameSet.choiceBtnClip, false);
                break;
            case SoundUiTypes.MenuButton:
                PlayMusicFile(uiAudioSource, _ctx.gameSet.menuBtnClip, false);
                break;
            case SoundUiTypes.Timer:
                PlayMusicFile(timerAudioSource, _ctx.gameSet.timerClip, true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
    
    public void StopTimer()
    {
        timerAudioSource.Stop();
        timerAudioSource.clip = null;
    }

    public void PlaySfx(AudioClip audioClip)
    {
        var source = Instantiate<TempAudioSource>(tempSoundSourcePrefab);
        source.PlayAndDestroy(audioClip, () => { _sfxs.Remove(source); });

        _sfxs ??= new List<TempAudioSource>();
        _sfxs.Add(source);
    }

    public void PlayLoopSfx(AudioClip audioClip, bool stop)
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
        
        if(_loopSounds != null)
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
}