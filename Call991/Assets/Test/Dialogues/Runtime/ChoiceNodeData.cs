using System;
using System.Collections.Generic;

namespace Test.Dialogues
{
    [Serializable]
    public class ChoiceNodeData : AaNodeData
    {
        public string Choice;
        public List<ChoiceCaseData> Cases;
    }

    [Serializable]
    public class ChoiceCaseData
    {
        public bool And;
        public List<string> Cases;
    }
}