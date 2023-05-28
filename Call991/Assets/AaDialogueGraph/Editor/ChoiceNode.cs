using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class ChoiceNode : AaNode
    {
        public void Set(ChoiceNodeData data, string guid, List<string> choiceKeys)
        {
            Guid = guid;

            NodeType = AaNodeType.ChoiceNode;
            titleContainer.Add(new ChoicePopupField(choiceKeys, data.Choice));

            CreateInPort();
            CreateOutPort();

            var foldout = new Foldout();
            foldout.value = false;

            var caseElement = new CaseGroupElement();
            caseElement.Set(foldout, data.CaseData.Words, data.CaseData.Ends, data.CaseData.Counts);
            foldout.Add(caseElement);
            contentContainer.Add(foldout);
            foldout.AddToClassList("aa-ChoiceNode_extension-container");
        }
    }
}