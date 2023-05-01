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
        public struct Ctx
        {
            public AudioSource audioSource;
        }

        private Ctx _ctx;
        private AudioClip _audioClip;
        private CompositeDisposable _disposables;
        
        public PhraseSoundPlayer(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();
        }

        public void PlayPhrase(AudioClip clip)
        {
            if (clip == null) return;

            Debug.Log($"[{this}] play phrase audio clip {_audioClip}");
            _ctx.audioSource.clip = clip;
            _ctx.audioSource.Play();
            _ctx.audioSource.loop = false;
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