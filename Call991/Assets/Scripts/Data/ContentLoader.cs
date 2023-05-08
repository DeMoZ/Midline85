using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AaDialogueGraph;
using UnityEngine;

namespace Data
{
    public class ContentLoader : IDisposable
    {
        public struct Ctx
        {
            public List<string> Languages;
            public PlayerProfile Profile;
        }

        private Ctx _ctx;

        public ContentLoader(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async Task<Phrase> GetPhraseAsync(PhraseNodeData data)
        {
            if (_ctx.Languages == null || _ctx.Languages.Count == 0) return null;

            var index = _ctx.Languages.IndexOf(_ctx.Profile.TextLanguage);

            if (index == -1) return null;

            Phrase result = null;

            result = await NodeUtils.GetObjectByPathAsync<Phrase>(data.Phrases[index]);

            if (result == null)
            {
                result = await NodeUtils.GetObjectByPathAsync<Phrase>(data.Phrases[0]);
            }

            return result;
        }

        public async Task<AudioClip> GetVoiceAsync(PhraseNodeData data)
        {
            if (_ctx.Languages == null || _ctx.Languages.Count == 0) return null;

            var index = _ctx.Languages.IndexOf(_ctx.Profile.AudioLanguage);

            if (index == -1) return null;

            AudioClip result = null;

            result = await NodeUtils.GetObjectByPathAsync<AudioClip>(data.PhraseSounds[index]);

            if (result == null)
            {
                result = await NodeUtils.GetObjectByPathAsync<AudioClip>(data.PhraseSounds[0]);
            }

            return result;
        }

        public async Task<T> GetObjectAsync<T>(string eventDataPhraseEvent) where T : UnityEngine.Object
        {
            var result = await NodeUtils.GetObjectByPathAsync<T>(eventDataPhraseEvent);
            return result;
        }

        public void Dispose()
        {
        }
    }
}