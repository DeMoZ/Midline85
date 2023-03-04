using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Dialogues
{
    public static class GraphElements
    {
        public static Port GeneratePort(PhraseNode node, Direction portDirection,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }
    }

    public class EntryPointNode : PhraseNode
    {
        public EntryPointNode()
        {
            title = "Start";
            DialogueText = "Entry Point";
            Guid = System.Guid.NewGuid().ToString();
            EntryPoint = true;

            var port = GraphElements.GeneratePort(this, Direction.Output);
            port.portName = "Next";
            outputContainer.Add(port);

            var addLanguageButton = new Button(() =>
            {
                var languageField = new LanguageField(onDelete: obj => { contentContainer.Remove(obj); });
                contentContainer.Add(languageField);
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
        public string Language;

        public LanguageField(string language = null, Action<VisualElement> onDelete = null)
        {
            language ??= "new";
            Language = language;

            var languageLabel = new Label(language);
            contentContainer.Add(languageLabel);

            var textField = new TextField
            {
                name = string.Empty,
                value = language,
            };

            textField.RegisterValueChangedCallback(evt =>
            {
                textField.name = evt.newValue;
                languageLabel.text = evt.newValue;
                Language = evt.newValue;
            });

            var deleteLanguageButton = new Button(() => { onDelete?.Invoke(this); })
            {
                text = "X",
            };

            contentContainer.Add(deleteLanguageButton);
            contentContainer.Add(languageLabel);
            contentContainer.Add(textField);
            contentContainer.style.flexDirection = FlexDirection.Row;
        }
    }

    public class PhraseAssetField : VisualElement
    {
        private ObjectField _objectField;
        
        public PhraseAssetField(Phrase phraseAsset = null)
        {
            _objectField = new ObjectField
            {
                objectType = typeof(Phrase),
                allowSceneObjects = false,
                value = phraseAsset,
            };
            
            contentContainer.Add(_objectField);
        }

        public Phrase GetPhrase()
        {
            return _objectField.value as Phrase;
        }
    }
}