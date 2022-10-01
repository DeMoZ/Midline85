using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Data
{
    public class PhraseEventSoundLoader : IDisposable
    {
        public struct Ctx
        {
            public string eventSoPath;
            public string streamingPath; // *strAssets*/Sounds/EventSounds
            public string resourcesPath; // *Resources*/Sounds/EventSounds
            public AudioManager audioManager;
        }

        private Ctx _ctx;
        private AudioClip _audioClip;

        private string _streamingPath;
        private string _audioPath;
        private CompositeDisposable _disposables;

        
        public PhraseEventSoundLoader(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            _streamingPath = "file:///" + Path.Combine(Application.streamingAssetsPath, _ctx.streamingPath);
        }

        public async Task LoadMusicEvent(string soundEventId)
        {
        }

        public async Task LoadSfxEvent(string soundEventId, bool loop, bool stop)
        {
            // Load audio event config from resources
            var soundConfig = await LoadConfig(soundEventId);

            if (soundConfig == null)
            {
                Debug.LogError($"[{this}] sound event SO wasn't found: {_ctx.eventSoPath}/{soundEventId}");
                return;
            }

            // Load audio file
            AudioClip audioClip;
            if (soundConfig.clip)
                audioClip = soundConfig.audioClip;
            else
                audioClip = await TryLoadStreamingSound(soundConfig.audioName);

            if (audioClip == null)
            {
                Debug.LogError($"[{this}] AudioFile {soundConfig.audioName} wasnt loaded ");
                return;
            }

            // Run audio manager
            if (loop)
            {
                 _ctx.audioManager.PlayLoopSfx(audioClip, stop);
                 ResourcesLoader.UnloadUnused();
            }
            else
            {
                _ctx.audioManager.PlaySfx(audioClip);
            }
        }

        private async Task<PhraseSfxEventSo> LoadConfig(string soundEventId)
        {
            var soFile = Path.Combine(_ctx.eventSoPath, soundEventId);
            var conf = await ResourcesLoader.LoadAsync<PhraseSfxEventSo>(soFile);
            return conf;
        }

        private async Task<AudioClip> TryLoadStreamingSound(string audioName)
        {
            _audioPath = Path.Combine(_streamingPath, audioName + ".wav");
            var isLoaded = false;
            Observable.FromCoroutine(LoadAudio).Subscribe(_ =>
            {
                Debug.Log($"[{this}] Sound load routine end: {_audioPath}");
                isLoaded = true;
            }).AddTo(_disposables);

            while (!isLoaded)
                await Task.Yield();

            return _audioClip;
        }

        private IEnumerator LoadAudio()
        {
            _audioClip = null;

            var request = UnityWebRequestMultimedia.GetAudioClip(_audioPath, AudioType.WAV);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[{this}] audio wasn't loaded: {_audioPath}\n{request.error}");
                yield break;
            }
            else
            {
                _audioClip = DownloadHandlerAudioClip.GetContent(request);
            }
        }
        
        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}