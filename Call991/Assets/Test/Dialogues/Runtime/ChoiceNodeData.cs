using System;
using System.Collections.Generic;

namespace Test.Dialogues
{
    [Serializable]
    public class ChoiceNodeData : AaNodeData
    {
        public List<ChoiceData> Choices;
    }

    [Serializable]
    public class ChoiceData
    {
        public string KeyCode;
        public List<string> Cases;
    }
}