using System;
using UniRx;
using UnityEngine;

namespace Data
{
    public class PhraseSoundPlayer : IDisposable
    {
        public struct Ctx
        {
            public AudioSource AudioSource;
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
            _ctx.AudioSource.clip = clip;
            _ctx.AudioSource.Play();
            _ctx.AudioSource.loop = false;
        }
        
        public void Pause(bool pause)
        {
            if (pause)
                _ctx.AudioSource.Pause();
            else
                _ctx.AudioSource.UnPause();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}