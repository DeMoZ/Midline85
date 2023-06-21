using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class ChoiceNode : AaNode
    {
        public void Set(ChoiceNodeData data, string guid)
        {
            Guid = guid;

            NodeType = AaNodeType.ChoiceNode;
            titleContainer.Add(new ChoicePopupField(AaKeys.ChoiceKeys, data.Choice));

            CreateInPort();
            CreateOutPort();

            var caseFoldout = new Foldout();
            caseFoldout.value = false;

            var caseElement = new CaseGroupElement();
            caseElement.Set(caseFoldout, data.CaseData.Words, data.CaseData.Ends, data.CaseData.Counts);
            caseFoldout.Add(caseElement);
            contentContainer.Add(caseFoldout);
            caseFoldout.AddToClassList("aa-ChoiceNode_extension-container");
        }
    }
}