using System;
using System.Threading.Tasks;
using UniRx;

namespace Data
{
    public class PhraseEventVideoLoader : IDisposable
    {
        public struct Ctx
        {
            public string eventSoPath;
            public string streamingPath; // *strAssets*/Sounds/EventSounds
            public string resourcesPath; // *Resources*/Sounds/EventSounds
            public VideoManager audioManager;
        }

        private Ctx _ctx;
       
        private CompositeDisposable _disposables;

        
        public PhraseEventVideoLoader(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();
        }
        public void Dispose()
        {
        }

        public void LoadVfxEvent(string eventId, bool b, bool b1)
        {
            throw new NotImplementedException();
        }

        public async Task LoadVideoEvent(string confSoundEventId)
        {
            throw new NotImplementedException();
        }
    }
}