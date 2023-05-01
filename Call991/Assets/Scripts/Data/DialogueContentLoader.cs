using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AaDialogueGraph;
using UnityEngine;

namespace Data
{
    public class DialogueContentLoader : IDisposable
    {
        public struct Ctx
        {
            public List<string> Languages;
            public PlayerProfile Profile;
        }

        private Ctx _ctx;

        public DialogueContentLoader(Ctx ctx)
        {
            _ctx = ctx;
        }

        public Phrase GetPhrase(PhraseNodeData data)
        {
            if (_ctx.Languages == null || _ctx.Languages.Count == 0) return null;

            var index = _ctx.Languages.IndexOf(_ctx.Profile.TextLanguage.ToString());

            if (index == -1) return null;

            Phrase result = null;

            // TODO this loading should be awaitable and asynchronous
            result = NodeUtils.GetObjectByPath<Phrase>(data.Phrases[index])
                     ?? NodeUtils.GetObjectByPath<Phrase>(data.Phrases[0]);

            return result;
        }

        public AudioClip GetVoice(PhraseNodeData data)
        {
            if (_ctx.Languages == null || _ctx.Languages.Count == 0) return null;

            var index = _ctx.Languages.IndexOf(_ctx.Profile.AudioLanguage);

            if (index == -1) return null;

            AudioClip result = null;

            // TODO this loading should be awaitable and asynchronous
            result = NodeUtils.GetObjectByPath<AudioClip>(data.PhraseSounds[index])
                     ?? NodeUtils.GetObjectByPath<AudioClip>(data.PhraseSounds[0]);

            return result;
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

        public void Dispose()
        {
        }
    }
}