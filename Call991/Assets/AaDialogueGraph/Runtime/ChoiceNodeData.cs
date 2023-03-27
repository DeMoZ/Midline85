using System;
using System.Collections.Generic;

namespace AaDialogueGraph
{
    [Serializable]
    public class ChoiceNodeData : AaNodeData
    {
        public string Choice;
        public List<CaseData> Cases;
    }

    [Serializable]
    public class CaseData
    {
        public bool And;
        public List<string> Cases;
    }
}