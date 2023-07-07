using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class NewspaperNode : AaNode
    {
        public void Set(NewspaperNodeData data, List<string> languages, string guid, 
            List<string> sounds, List<string> musics)
        {
            Guid = guid;
            title = AaGraphConstants.NewspaperNode;
            
            var contentFolder = new Foldout();
            contentFolder.value = false;
            extensionContainer.Add(contentFolder);
            contentFolder.AddToClassList("aa-NewspaperNode_extension-container");
            
            var nodeEvents = new AaNodeEvents();
            nodeEvents.Set(data.EventVisualData, CheckNodeContent, sounds, musics);
            contentFolder.Add(nodeEvents);

            var phraseContainer = new ElementsTable();
            var phraseAssetsLabel = new Label("Images");
            phraseAssetsLabel.AddToClassList("aa-BlackText");
            phraseContainer.Add(phraseAssetsLabel);

            for (var i = 0; i < languages.Count; i++)
            {
                var sprite = data.Sprites != null && data.Sprites.Count > i
                    ? NodeUtils.GetObjectByPath<Sprite>(data.Sprites[i])
                    : null;
                
                var field = new NewspaperElementsRowField();
                field.Set(languages[i], sprite, CheckNodeContent);
                phraseContainer.Add(field);
            }

            phraseContainer.contentContainer.AddToClassList("aa-PhraseAsset_content-container");

            contentFolder.Add(phraseContainer);

            CreateInPort();
            CreateOutPort();

            RefreshExpandedState();
            RefreshPorts();
            CheckNodeContent();
        }

        public void CheckNodeContent()
        {
           
        }

        public List<Sprite> GetSprites() =>
            contentContainer.Query<SpriteField>().ToList().Select(field => field.GetSprite()).ToList();
    }
}