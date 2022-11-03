using System;
using System.IO;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace Data
{
    public class PhraseEventVideoLoader : IDisposable
    {
        public struct Ctx
        {
            public string eventSoPath;
            public string streamingPath; // *strAssets*/Sounds/EventSounds
            public string resourcesPath; // *Resources*/Sounds/EventSounds
            public VideoManager videoManager;
        }

        private Ctx _ctx;
       
        private CompositeDisposable _disposables;
        private string _streamingPath;
        private string _videoPath;
        private VideoClip _videoClip;
        
        public PhraseEventVideoLoader(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();
            
            _streamingPath = "file:///" + Path.Combine(Application.streamingAssetsPath, _ctx.streamingPath);
        }
        public void Dispose()
        {
            ResourcesLoader.UnloadUnused();
        }

        public async Task LoadVfxEvent(string eventId, bool b, bool b1)
        {
            var config = await LoadConfig(eventId);

            if (config == null)
            {
                Debug.LogError($"[{this}] sound event SO wasn't found:  A PATH /{eventId}");
                return;
            }

            if (config.clip)
            {
                _ctx.videoManager.PlayVideo(config.videoClip , PhraseEventTypes.Vfx);
            }
            else
            {
                _videoPath = Path.Combine(_streamingPath, config.videoName + ".mp4");
                _ctx.videoManager.PlayVideo(_videoPath,  PhraseEventTypes.Vfx);
            }
        }

        public async Task LoadVideoEvent(string eventId)
        {
           
            var config = await LoadConfig(eventId);

            if (config == null)
            {
                Debug.LogError($"[{this}] sound event SO wasn't found: A PATH /{eventId}");
                return;
            }
            
            if (config.clip)
            {
                _ctx.videoManager.PlayVideo(config.videoClip , PhraseEventTypes.LoopVfx);
            }
            else
            {
                _videoPath = Path.Combine(_streamingPath, config.videoName + ".mp4");
                _ctx.videoManager.PlayVideo(_videoPath,  PhraseEventTypes.LoopVfx);
            }
        }

        public async Task LoadVideoTitle(string titleVideoSoName)
        {
            //var path = Path.Combine(_ctx.streamingPath, titleVideoSoName);
            var config = await ResourcesLoader.LoadAsync<PhraseVfxEventSo>(titleVideoSoName);
            
            if (config == null)
            {
                Debug.LogError($"[{this}] sound event SO wasn't found:  A PATH /{titleVideoSoName}");
                return;
            }

            Debug.Log($"[{this}] LoadVideoTitle {config.eventId} {PhraseEventTypes.LoopVfx}");
            if (config.clip)
            {
                _ctx.videoManager.PlayVideo(config.videoClip , PhraseEventTypes.LoopVfx);
            }
            else
            {
                _videoPath = Path.Combine(_streamingPath, config.videoName + ".mp4");
                _ctx.videoManager.PlayVideo(_videoPath,  PhraseEventTypes.LoopVfx);
            }
        }

        private async Task<PhraseVfxEventSo> LoadConfig(string eventId)
        {
            var soFile = Path.Combine(_ctx.eventSoPath, eventId);
            var conf = await ResourcesLoader.LoadAsync<PhraseVfxEventSo>(soFile);
            return conf;
        }
    }
}