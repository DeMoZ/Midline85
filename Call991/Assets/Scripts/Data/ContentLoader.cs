using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AaDialogueGraph;
using ContentDelivery;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data
{
    public class ContentLoader : IDisposable
    {
        public struct Ctx
        {
            public List<string> LevelLanguages;
            public PlayerProfile Profile;
            public AddressableDownloader AddressableDownloader;
        }

        private Ctx _ctx;

        public ContentLoader(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async Task<Phrase> GetPhraseAsync(PhraseNodeData data)
        {
            if (_ctx.LevelLanguages == null || _ctx.LevelLanguages.Count == 0) return null;

            var index = _ctx.LevelLanguages.IndexOf(_ctx.Profile.TextLanguage);

            if (index == -1) index = 0;

            Phrase result = null;

            result = await NodeUtils.GetObjectByPathAsync<Phrase>(data.Phrases[index]);

            if (result == null && index != 0)
            {
                result = await NodeUtils.GetObjectByPathAsync<Phrase>(data.Phrases[0]);
            }

            return result;
        }
        
        public async Task<Phrase> GetPhraseAsync(ImagePhraseNodeData data)
        {
            if (_ctx.LevelLanguages == null || _ctx.LevelLanguages.Count == 0) return null;

            var index = _ctx.LevelLanguages.IndexOf(_ctx.Profile.TextLanguage);

            if (index == -1) index = 0;

            Phrase result = null;

            result = await NodeUtils.GetObjectByPathAsync<Phrase>(data.Phrases[index]);

            if (result == null && index != 0)
            {
                result = await NodeUtils.GetObjectByPathAsync<Phrase>(data.Phrases[0]);
            }

            return result;
        }
        
        public async Task<Sprite> GetSlideAsync(SlideNodeData data)
        {
            if (_ctx.LevelLanguages == null || _ctx.LevelLanguages.Count == 0) return null;

            var index = _ctx.LevelLanguages.IndexOf(_ctx.Profile.TextLanguage);

            if (index == -1) index = 0;

            Sprite result = null;

            result = await NodeUtils.GetObjectByPathAsync<Sprite>(data.Slides[index]);

            if (result == null && index != 0) 
                result = await NodeUtils.GetObjectByPathAsync<Sprite>(data.Slides[0]);

            return result;
        }
        
        public async Task<Sprite> GetSpriteAsync(string path)
        {
            return await NodeUtils.GetObjectByPathAsync<Sprite>(path);
        }
        
        public async Task<CompositeNewspaper> GetNewspaperAsync(NewspaperNodeData data)
        {
            if (_ctx.LevelLanguages == null || _ctx.LevelLanguages.Count == 0) return null;

            var index = _ctx.LevelLanguages.IndexOf(_ctx.Profile.TextLanguage);

            if (index == -1) return null;

            var result  = await NodeUtils.GetObjectByPathAsync<CompositeNewspaper>(data.NewspaperPrefab);
            
            return result;
        }

        public async Task<T> GetObjectAsync<T>(string eventDataPhraseEvent) where T : UnityEngine.Object
        {
            return await NodeUtils.GetObjectByPathAsync<T>(eventDataPhraseEvent);
        }

        public async Task<object> GetAddressableAsync<T>(string key, Action<float> onProgress, CancellationToken token)
        {
            return await _ctx.AddressableDownloader.DownloadAsync<T>(key, onProgress, token);
        }

        public void ReleaseAddressable(UnityEngine.Object obj)
        {
           Addressables.Release(obj);
        }
        
        public void Dispose()
        {
        }
    }
}