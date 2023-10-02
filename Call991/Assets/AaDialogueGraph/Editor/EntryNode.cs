using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class EntryNode : AaNode
    {
        public void Set(AaReactive<LanguageOperation> onLanguageChange, EntryNodeData data)
        {
            title = "Start";

            Guid = data.Guid ?? System.Guid.NewGuid().ToString();
            NodeType = AaNodeType.EntryNode;

            var port = GraphElements.GeneratePort(this, Direction.Output);
            port.portName = "Next";
            outputContainer.Add(port);

            var levelId = AaKeys.LevelIdKeys.Contains(data.LevelId) ? data.LevelId : null;
            var levelKey = new LevelIdPopupField(AaKeys.LevelIdKeys, levelId);
            contentContainer.Add(levelKey);

            // var soundLabel = new Label("Sounds");
            // var soundAsset = string.IsNullOrEmpty(data.SoundAsset)
            //     ? null
            //     : NodeUtils.GetObjectByPath<WwiseSoundsKeysList>(data.SoundAsset);
            //
            // var soundField = new SoundAssetField();
            // soundField.Set(soundAsset);
            // var soundAssetLineGroup = new LineGroup(new VisualElement[] { soundLabel, soundField });
            // contentContainer.Add(soundAssetLineGroup);
            
            var btnLabel = new Label("btn.Filter");
            var buttonFilterField = new ButtonFilterTextField
            {
                value = data.ButtonFilter
            };
            
            var grabProjectorImages = new Toggle
            {
                text = AaGraphConstants.ProjectorImages,
                value = data?.GrabProjectorImages ?? false,
                tooltip = "Dialogue has projector images and \n  dont have loops. \n Interlude only",
                name = AaGraphConstants.ProjectorImages,
            };
            contentContainer.Add(grabProjectorImages);
            
            var enableSkipButton = new Toggle
            {
                text = AaGraphConstants.EnableSkipLevelButton,
                value = data?.EnableSkipLevelButton ?? false,
                tooltip = "Dialogue will bi skipped to the end node \n used for cinematic only",
                name = AaGraphConstants.EnableSkipLevelButton,
            };
            contentContainer.Add(enableSkipButton);

            var btnFilterLineGroup = new LineGroup(new VisualElement[] { btnLabel, buttonFilterField });
            contentContainer.Add(btnFilterLineGroup);

            var addLanguageButton = new Button(() =>
            {
                var currentLanguage = AaKeys.LanguageKeys[0];
                var languageField = new LanguageField();
                languageField.Set(currentLanguage, onLanguageChange);
                contentContainer.Add(languageField);

                onLanguageChange.Value = new LanguageOperation
                {
                    Type = LanguageOperationType.Add,
                    Value = currentLanguage
                };
            });
            addLanguageButton.text = "Add Language";
            contentContainer.Add(addLanguageButton);

            RefreshExpandedState();
            RefreshPorts();

            SetPosition(new Rect(100, 200, 200, 150));
        }

        public string GetFilters()
        {
            return contentContainer.Q<ButtonFilterTextField>().value;
        }
        
        public List<string> GetLanguages()
        {
            var languages = new List<string>();
            var languageFields = contentContainer.Query<LanguageField>();

            languageFields.ForEach(lf => languages.Add(lf.Language));
            return languages;
        }
    }

    public class LanguageField : VisualElement
    {
        public string Language { get; private set; }

        public void Set(string language, AaReactive<LanguageOperation> onLanguage)
        {
            var deleteLanguageButton = new Button(() =>
            {
                onLanguage.Value = new LanguageOperation
                {
                    Type = LanguageOperationType.Remove,
                    Element = this
                };
            })
            {
                text = "X",
            };
            contentContainer.Add(deleteLanguageButton);

            Language = language;
            var popup = new LanguagePopupField(AaKeys.LanguageKeys, language);
            popup.OnValueChange += (newValue) =>
            {
                Language = newValue;

                onLanguage.Value = new LanguageOperation
                {
                    Type = LanguageOperationType.Change,
                    Value = newValue,
                    Element = this
                };
            };
            contentContainer.Add(popup);

            contentContainer.style.flexDirection = FlexDirection.Row;
        }
    }
}