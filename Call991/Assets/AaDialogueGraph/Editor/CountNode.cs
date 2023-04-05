using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class CountNode : AaNode
    {
        public CountNode(CountNodeData data, string guid)
        {
            Guid = guid;
            NodeType = AaNodeType.CountNode;
            titleContainer.Add(new CountPopupField(AaKeys.CountKeys, data.Choice));

            CreateInPort();
            CreateOutPort();

            var foldout = new Foldout();
            foldout.value = false;
            foldout.contentContainer.style.flexDirection = FlexDirection.Row;
            foldout.AddToClassList("aa-CountNode_extension-container");
            contentContainer.Add(foldout);
            
            foldout.text = ValueToShowStyle(data.Value);
            
            foldout.Add(new Label("Value"));

            var inputField = new IntegerField();
            inputField.value = data.Value;
            inputField.style.width = 40;
            inputField.RegisterValueChangedCallback(evt => { foldout.text = ValueToShowStyle(evt.newValue); });
            foldout.Add(inputField);
        }
        
        private string ValueToShowStyle( int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }
    }
}