using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Data
{
    public class PhraseSoundPlayer : IDisposable
    {
        private AudioClip _audioClip;
        private string _path;
        private string _audioPath;

        private Ctx _ctx;
        private CompositeDisposable _disposables;

        public struct Ctx
        {
            public string streamingPath; // *strAssets*/Sounds/RU/RU_7_P
            public string resourcesPath;
            public AudioSource audioSource;
        }

        public PhraseSoundPlayer(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            _path = "file:///" + Path.Combine(Application.streamingAssetsPath, _ctx.streamingPath);
        }

         public async Task TryLoadStreamingDialogue(string phraseName)
         {
             _audioPath = Path.Combine(_path, phraseName + ".wav");
             //_audioPath = Path.Combine(_path, phraseName + ".ogg");
             var isLoaded = false;
             Observable.FromCoroutine(LoadAudio).Subscribe(_ =>
             {
                 Debug.Log($"[{this}] Phrase sound load routine end: {_audioPath}");
                 isLoaded = true;
             }).AddTo(_disposables);
        
             while (!isLoaded)
                 await Task.Yield();
         }

        public async Task TryLoadDialogue(string phraseName)
        {
            _audioClip = null;
            var clip = await ResourcesLoader.LoadAsync<AudioClip>(Path.Combine(_ctx.resourcesPath, phraseName));
            if (clip) _audioClip = clip;
        }

        public void TryPlayPhraseFile()
        {
            if (_audioClip == null) return;

            Debug.Log($"[{this}] play phrase audio clip {_audioClip}");
            _ctx.audioSource.clip = _audioClip;
            _ctx.audioSource.Play();
            _ctx.audioSource.loop = false;
        }

        private IEnumerator LoadAudio()
        {
            _audioClip = null; // todo: set to default audio with noise

            var request = UnityWebRequestMultimedia.GetAudioClip(_audioPath, AudioType.WAV);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) //.ConnectionError)
            {
                Debug.LogError($"[{this}] audio wasn't loaded: {_audioPath}\n{request.error}");
                yield break;
            }
            else
            {
                _audioClip = DownloadHandlerAudioClip.GetContent(request);
            }
        }

        public void Pause(bool pause)
        {
            if (pause)
                _ctx.audioSource?.Pause();
            else
                _ctx.audioSource?.UnPause();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}