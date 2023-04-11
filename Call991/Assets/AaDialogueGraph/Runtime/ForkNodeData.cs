using System.Collections.Generic;

namespace AaDialogueGraph
{
    [System.Serializable]
    public class ForkNodeData : AaNodeData
    { 
        public List<ForkCaseData> ForkCaseData = new ();
    }

    [System.Serializable]
    public class ForkCaseData : CaseData
    {
        public string ForkExitName;

        public ForkCaseData(CaseData caseData, string name) : base (caseData.Words, caseData.Ends, caseData.Counts)
        {
            ForkExitName = name;
        } 
    }
}