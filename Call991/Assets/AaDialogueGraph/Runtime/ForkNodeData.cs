using System;
using System.Collections;
using System.Collections.Generic;

namespace AaDialogueGraph
{
    [Serializable]
    public class ForkNodeData : AaNodeData
    {
        public string Choice;
        public List<ExitCaseData> Exits { get; set; }
    }

    [Serializable]
    public class ExitCaseData
    {
        public List<CaseData> Cases;

    }
}