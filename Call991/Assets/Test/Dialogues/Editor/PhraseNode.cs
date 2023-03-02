using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Test.Dialogues
{
    public class PhraseNode : Node
    {
        public string Guid;
        public string DialogueText;
        public bool EntryPoint = false;
        public List<string> Languages = new ();
    }

    public class ChoiceNode : Node
    {
        
    }
}