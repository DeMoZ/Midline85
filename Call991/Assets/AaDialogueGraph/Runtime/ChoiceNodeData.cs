using System;
using System.Collections.Generic;
using UnityEngine;

namespace AaDialogueGraph
{
    [Serializable]
    public class ChoiceNodeData : AaNodeData
    {
        public string Choice;
        public List<CaseData> Words;
        public List<EndData> Ends;
        public List<CountData> Counts;
    }

    [Serializable]
    public class CaseData
    {
        public CaseType CaseType;
        public List<string> OrKeys;
    }

    [Serializable]
    public class EndData
    {
        public EndType EndType;
        public List<string> OrKeys;
    }

    [Serializable]
    public class CountData
    {
        public CountType CountType;
        public string CountKey;
        public Vector2Int Range;
    }
}