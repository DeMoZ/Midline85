using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class EventNode : AaNode
    {
        public void Set(EventNodeData data, string guid, List<string> sounds, List<string> musics, List<string> rtpcs)
        {
            Guid = guid;
            title = AaGraphConstants.EventNode;
            
            titleContainer.Add(new NodeTitleErrorField());

            var contentFolder = new Foldout();
            contentFolder.value = false;
            extensionContainer.Add(contentFolder);
            contentFolder.AddToClassList("aa-EventAsset_content-container");

            var phraseEvents = new AaNodeEvents();
            phraseEvents.Set(data.EventVisualData, CheckNodeContent, sounds, musics, rtpcs);
            contentFolder.Add(phraseEvents);
            
            CreateInPort();
            CreateOutPort();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void CheckNodeContent()
        {
        }
    }
}