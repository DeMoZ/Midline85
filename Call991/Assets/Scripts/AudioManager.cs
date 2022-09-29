using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Configs;
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

    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private TempAudioSource tempSoundSourcePrefab;
    [SerializeField] private LoopAudioSource loopSoundSourcePrefab;

    private string _languagePath;
    private Ctx _ctx;
    private string _currentMusicPath;
    private AudioClip _currentMusicClip;
    private Dictionary<string, LoopAudioSource> _loopSounds;

    public void SetCtx(Ctx ctx)
    {
        _ctx = ctx;
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
                PlayMusicFile(uiAudioSource, _ctx.gameSet.timerClip, true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void PlaySfx(AudioClip audioClip)
    {
        var source = Instantiate<TempAudioSource>(tempSoundSourcePrefab);
        source.PlayAndDestroy(audioClip);
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
            if (stop)
            {
            }
            else
            {
                source = Instantiate<LoopAudioSource>(loopSoundSourcePrefab);
                source.Play(audioClip);
                _loopSounds.Add(audioName, source);
            }
        }
    }

    public void OnSceneSwitch()
    {
        _loopSounds ??= new Dictionary<string, LoopAudioSource>();

        foreach (var loop in _loopSounds)
        {
            if(loop.Value!=null)
                loop.Value.Destroy();
        }
        
        _loopSounds.Clear();
    }
}