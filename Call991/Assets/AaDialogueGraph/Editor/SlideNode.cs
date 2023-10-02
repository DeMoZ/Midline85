using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class SlideNode : AaNode
    {
        public void Set(SlideNodeData data, List<string> languages, string guid, SoundLists soundLists)
        {
            Guid = guid;
            title = AaGraphConstants.SlideNode;

            var contentFolder = new Foldout();
            contentFolder.value = false;
            extensionContainer.Add(contentFolder);
            contentFolder.AddToClassList("aa-NewspaperNode_extension-container");

            var nodeEvents = new AaNodeEvents();
            nodeEvents.Set(data.EventVisualData, CheckNodeContent, soundLists);
            contentFolder.Add(nodeEvents);
            
            var slidesContainer = new ElementsTable();
            contentFolder.Add(slidesContainer);
            
            var label = new Label(AaGraphConstants.Slides);
            slidesContainer.Add(label);
            
            for (var i = 0; i < languages.Count; i++)
            {
                var slide = data.Slides != null && data.Slides.Count > i
                    ? NodeUtils.GetObjectByPath<Sprite>(data.Slides[i])
                    : null;
                
                var objectField = new ObjectField
                {
                    objectType = typeof(Sprite),
                    allowSceneObjects = false,
                    value = slide,
                    name = AaGraphConstants.SlideNode,
                };

                var langLabel = new Label(languages[i]);
                langLabel.AddToClassList("aa-BlackText");
                var slideGroup = new  LineGroup(new List<VisualElement> { langLabel, objectField });
                slidesContainer.Add(slideGroup);
            }
            
            slidesContainer.contentContainer.AddToClassList("aa-PhraseAsset_content-container");
            
            CreateInPort();
            CreateOutPort();

            RefreshExpandedState();
            RefreshPorts();
            CheckNodeContent();
        }

        public void CheckNodeContent()
        {
        }

        public List<Sprite> GetSlides()
        {
            var fields = contentContainer.Query<ObjectField>(AaGraphConstants.SlideNode).ToList();
            return fields.Select(field => field.value as Sprite).ToList();
        }
    }
}