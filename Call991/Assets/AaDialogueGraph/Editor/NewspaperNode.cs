using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class NewspaperNode : AaNode
    {
        private CompositeNewspaper _objectField;

        public void Set(NewspaperNodeData data, List<string> languages, string guid, SoundLists soundLists)
        {
            Guid = guid;
            title = AaGraphConstants.NewspaperNode;

            var contentFolder = new Foldout();
            contentFolder.value = false;
            extensionContainer.Add(contentFolder);
            contentFolder.AddToClassList("aa-NewspaperNode_extension-container");

            var nodeEvents = new AaNodeEvents();
            nodeEvents.Set(data.EventVisualData, CheckNodeContent, soundLists);
            contentFolder.Add(nodeEvents);
            
            var label = new Label(AaGraphConstants.NewspaperObject);
            label.AddToClassList("aa-BlackText");
            
            var compositeNewspaper = !string.IsNullOrEmpty(data.NewspaperPrefab)
                ? NodeUtils.GetObjectByPath<CompositeNewspaper>(data.NewspaperPrefab)
                : null;

            var newspaperAsset = new NewspaperAssetField { name = AaGraphConstants.NewspaperObject };
            newspaperAsset.Set(compositeNewspaper);
            
            var lineGroup = new LineGroup(new List<VisualElement> { label, newspaperAsset });
            lineGroup.contentContainer.AddToClassList("aa-PhraseAsset_content-container");
            contentFolder.Add(lineGroup);

            CreateInPort();
            CreateOutPort();

            RefreshExpandedState();
            RefreshPorts();
            CheckNodeContent();
        }

        public void CheckNodeContent()
        {
        }
    }
}