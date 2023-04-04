using System.Collections.Generic;

namespace AaDialogueGraph
{
    [System.Serializable]
    public class ForkNodeData : AaNodeData
    { 
        public List<ForkCaseData> CaseData;
    }

    [System.Serializable]
    public class ForkCaseData : CaseData
    {
        public string ForkExitName;

        public ForkCaseData(CaseData caseData, string name)
        {
            ForkExitName = name;
        } 
    }
}