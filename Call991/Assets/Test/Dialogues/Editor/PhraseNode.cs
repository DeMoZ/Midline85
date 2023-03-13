using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Dialogues
{
    public class PhraseNode : AaNode
    {
        public string DialogueText;

        public List<AudioClip> GetPhraseSounds() => 
            contentContainer.Query<PhraseSoundField>().ToList().Select(field=>field.GetPhraseSound()).ToList();

        public List<Phrase> GetPhrases() => 
            contentContainer.Query<PhraseAssetField>().ToList().Select(field=>field.GetPhrase()).ToList();

        public PersonVisual GetPersonVisual() => 
            contentContainer.Query<PersonVisual>().First();

        public PhraseVisual GetPhraseVisual() => 
            contentContainer.Query<PhraseVisual>().First();

        public List<EventVisual> GetEventsVisual() => 
            contentContainer.Query<EventVisual>().ToList();
    }
}