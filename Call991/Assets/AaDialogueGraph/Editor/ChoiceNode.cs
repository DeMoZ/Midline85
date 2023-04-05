using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class ChoiceNode : AaNode
    {
        public ChoiceNode(ChoiceNodeData data, string guid)
        {
            Guid = guid;

            NodeType = AaNodeType.ChoiceNode;
            titleContainer.Add(new ChoicePopupField(AaKeys.ChoiceKeys, data.Choice));

            CreateInPort();
            CreateOutPort();

            var caseFoldout = new Foldout();
            caseFoldout.value = false;
            
            var caseElement = new CaseGroupElement(caseFoldout, data.CaseData.Words, data.CaseData.Ends, data.CaseData.Counts, null);
            caseFoldout.Add(caseElement);
            contentContainer.Add(caseFoldout);
            caseFoldout.AddToClassList("aa-ChoiceNode_extension-container");
        }
    }
}