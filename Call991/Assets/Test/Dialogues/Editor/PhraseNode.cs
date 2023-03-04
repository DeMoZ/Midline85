using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Test.Dialogues
{
    public class PhraseNode : Node
    {
        public string Guid;
        public string DialogueText;
        public bool EntryPoint = false;

        public List<Phrase> GetPhrases()
        {
            return contentContainer.Query<PhraseAssetField>().ToList().Select(f=>f.GetPhrase()).ToList();
        }
    }

    public class ChoiceNode : Node
    {
    }
}