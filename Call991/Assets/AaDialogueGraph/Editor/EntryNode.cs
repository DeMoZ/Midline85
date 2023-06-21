using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class EntryNode : AaNode
    {
        public void Set(AaReactive<LanguageOperation> onLanguageChange, string guid = null)
        {
            title = "Start";
            Guid = guid ?? System.Guid.NewGuid().ToString();
            NodeType = AaNodeType.EntryPoint;

            var port = GraphElements.GeneratePort(this, Direction.Output);
            port.portName = "Next";
            outputContainer.Add(port);

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