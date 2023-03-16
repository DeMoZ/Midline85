using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using I2.Loc;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Object = UnityEngine.Object;
using Toggle = UnityEngine.UIElements.Toggle;

namespace Test.Dialogues
{
    public enum AaNodeType
    {
        Phrase,
        EntryPoint,
        ChoiceNode,
        MultiPhrase,
    }

    public enum LanguageOperationType
    {
        Add,
        Change,
        Remove,
    }

    public static class AaGraphConstants
    {
        public const string DefaultFileName = "NewDialogue";
        public const string NewLanguageName = "new";

        public const string DialogueGraph = "Dialogue Graph";
        public const string NewChoiceNode = "Choices Node";
        public const string NewPhraseNode = "Phrase Node";

        public const string SaveData = "Save Data";
        public const string LoadData = "Load Data";

        public const string LineSpace = " | ";
        public const string InPortName = "in";
        public const string OutPortName = "out";

        public const string NoData = "NO DATA";
    }

    public class LanguageOperation
    {
        public LanguageOperationType Type;
        public string Value;
        public VisualElement Element;
    }

    public static class GraphElements
    {
        public static Port GeneratePort(AaNode node, Direction portDirection,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }
    }

    public class EntryPointNode : AaNode
    {
        public EntryPointNode(AaReactive<LanguageOperation> onLanguageChange, string guid = null)
        {
            title = "Start";
            Guid = guid ?? System.Guid.NewGuid().ToString();
            NodeType = AaNodeType.EntryPoint;

            var port = GraphElements.GeneratePort(this, Direction.Output);
            port.portName = "Next";
            outputContainer.Add(port);

            var addLanguageButton = new Button(() =>
            {
                var languageField = new LanguageField(AaGraphConstants.NewLanguageName, onLanguageChange);
                contentContainer.Add(languageField);

                onLanguageChange.Value = new LanguageOperation
                {
                    Type = LanguageOperationType.Add,
                    Value = AaGraphConstants.NewLanguageName
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
        public string Language;

        public LanguageField(string language, AaReactive<LanguageOperation> onLanguage)
        {
            language ??= AaGraphConstants.NewLanguageName;
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

                onLanguage.Value = new LanguageOperation
                {
                    Type = LanguageOperationType.Change,
                    Value = evt.newValue,
                    Element = this
                };
            });

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
            contentContainer.Add(languageLabel);
            contentContainer.Add(textField);
            contentContainer.style.flexDirection = FlexDirection.Row;
        }
    }

    /// <summary>
    /// Helper element to be able to find contaniter with phrases
    /// </summary>
    public class PhraseElementsTable : VisualElement
    {
    }

    public class PhraseElementsRowField : VisualElement
    {
        public PhraseElementsRowField(string language, Object clip = null, Phrase phrase = null, Action onChange = null)
        {
            contentContainer.Add(new Label(language));
            contentContainer.Add(new PhraseSoundField(clip, onChange));
            contentContainer.Add(new PhraseAssetField(phrase, onChange));
            contentContainer.style.flexDirection = FlexDirection.Row;
        }
    }

    public class PhraseSoundField : VisualElement
    {
        private ObjectField _objectField;

        public PhraseSoundField(Object clip = null, Action onChange = null)
        {
            _objectField = new ObjectField
            {
                objectType = typeof(AudioClip),
                allowSceneObjects = false,
                value = clip,
            };

            _objectField.RegisterValueChangedCallback(_ => { onChange?.Invoke(); });

            contentContainer.Add(_objectField);
        }

        public AudioClip GetPhraseSound()
        {
            return _objectField.value as AudioClip;
        }
    }

    public class PhraseAssetField : VisualElement
    {
        private ObjectField _objectField;

        public PhraseAssetField(Object phraseAsset = null, Action onChange = null)
        {
            _objectField = new ObjectField
            {
                objectType = typeof(Phrase),
                allowSceneObjects = false,
                value = phraseAsset,
            };

            _objectField.RegisterValueChangedCallback(_ => { onChange?.Invoke(); });

            contentContainer.Add(_objectField);
        }

        public Phrase GetPhrase()
        {
            return _objectField.value as Phrase;
        }
    }

    public class PersonVisual : VisualElement
    {
        public PersonVisual(PersonVisualData data = null, Action<string> onPersonChange = null)
        {
            var personOptions = Enum.GetValues(typeof(Person)).Cast<Person>().ToList();
            var personPopup = new PopupField<Person>("", personOptions, data?.Person ?? personOptions[0],
                (val) => OnPersonChange(val, onPersonChange));

            var positionOptions = Enum.GetValues(typeof(ScreenPlace)).Cast<ScreenPlace>().ToList();
            var positionPopup =
                new PopupField<ScreenPlace>("", positionOptions, data?.ScreenPlace ?? positionOptions[1]);

            var onEndToggle = new Toggle
            {
                text = "HideOnEnd",
                value = data?.HideOnEnd ?? false,
            };

            contentContainer.Add(personPopup);
            contentContainer.Add(positionPopup);
            contentContainer.Add(onEndToggle);

            contentContainer.style.flexDirection = FlexDirection.Row;
        }

        private string OnPersonChange(Person val, Action<string> onPersonChange = null)
        {
            onPersonChange?.Invoke(val.ToString());
            return val.ToString();
        }

        public PersonVisualData GetData()
        {
            return new PersonVisualData
            {
                Person = contentContainer.Query<PopupField<Person>>().First().value,
                ScreenPlace = contentContainer.Query<PopupField<ScreenPlace>>().First().value,
                HideOnEnd = contentContainer.Query<Toggle>().First().value,
            };
        }
    }

    public class PhraseVisual : VisualElement
    {
        public PhraseVisual(PhraseVisualData data)
        {
            var appearOptions = Enum.GetValues(typeof(TextAppear)).Cast<TextAppear>().ToList();
            var appearPopup = new PopupField<TextAppear>("", appearOptions, data?.TextAppear ?? appearOptions[0]);
            var onEndToggle = new Toggle
            {
                text = "HideOnEnd",
                value = data?.HideOnEnd ?? false,
            };

            contentContainer.Add(appearPopup);
            contentContainer.Add(onEndToggle);

            contentContainer.style.flexDirection = FlexDirection.Row;
        }

        public PhraseVisualData GetData()
        {
            return new PhraseVisualData
            {
                TextAppear = contentContainer.Query<PopupField<TextAppear>>().First().value,
                HideOnEnd = contentContainer.Query<Toggle>().First().value,
            };
        }
    }

    #region PhraseEvents

    public class PhraseEvents : VisualElement
    {
        private Action _onChange;

        public PhraseEvents(List<EventVisualData> data, Action onChange)
        {
            _onChange = onChange;

            var addEventAssetButton = new Button(() =>
            {
                contentContainer.Add(new EventVisual(new EventVisualData(), OnDeleteEvent, _onChange));
                _onChange?.Invoke();
            });
            addEventAssetButton.text = "Event Asset";
            contentContainer.Add(addEventAssetButton);

            data?.ForEach(item => contentContainer.Add(new EventVisual(item, OnDeleteEvent, _onChange)));
        }

        private void OnDeleteEvent(EventVisual eventVisual)
        {
            contentContainer.Remove(eventVisual);
            _onChange?.Invoke();
        }

        public List<EventVisual> GetEvents()
        {
            return contentContainer.Query<EventVisual>().ToList();
        }
    }

    public class EventVisual : VisualElement
    {
        public EventVisual(EventVisualData data, Action<EventVisual> onDelete, Action onChange)
        {
            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = "X",
            });

            var containerColumn = new VisualElement();
            contentContainer.Add(containerColumn);

            var containerRow1 = new VisualElement();
            containerRow1.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow1);

            containerRow1.Add(new EventAssetField(data.PhraseEventSo, onChange));

            var containerRow2 = new VisualElement();
            containerRow2.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow2);

            var typeOptions = Enum.GetValues(typeof(PhraseEventTypes)).Cast<PhraseEventTypes>().ToList();
            var typePopup = new PopupField<PhraseEventTypes>("", typeOptions, data?.EventType ?? typeOptions[2], val =>
            {
                onChange?.Invoke();
                return val.ToString();
            });
            containerRow2.Add(typePopup);

            var toggle = new Toggle
            {
                text = "Stop",
                value = data?.Stop ?? false,
            };
            containerRow2.Add(toggle);

            containerRow2.Add(new Label {text = "Delay"});
            var delay = new FloatField
            {
                value = data?.Delay ?? 0,
            };
            containerRow2.Add(delay);

            contentContainer.style.flexDirection = FlexDirection.Row;
        }

        public EventVisualData GetData()
        {
            return new EventVisualData
            {
                PhraseEventSo = contentContainer.Query<EventAssetField>().First().GetEvent(),
                EventType = contentContainer.Query<PopupField<PhraseEventTypes>>().First().value,
                Stop = contentContainer.Query<Toggle>().First().value,
                Delay = contentContainer.Query<FloatField>().First().value,
            };
        }
    }

    public class EventAssetField : VisualElement
    {
        private ObjectField _objectField;

        public EventAssetField(PhraseEventSo eventAsset = null, Action onChange = null)
        {
            _objectField = new ObjectField
            {
                objectType = typeof(PhraseEventSo),
                allowSceneObjects = false,
                value = eventAsset,
            };

            _objectField.RegisterValueChangedCallback(_ => onChange?.Invoke());

            contentContainer.Add(_objectField);
        }

        public PhraseEventSo GetEvent()
        {
            return _objectField.value as PhraseEventSo;
        }
    }

    #endregion

    public class NoEnumPopup : VisualElement
    {
        public NoEnumPopup(List<string> keys, Action<string> onChange)
        {
            contentContainer.Add(new PopupField<string>("", keys, keys?[0] ?? AaGraphConstants.NoData, val =>
            {
                onChange?.Invoke(val);
                return val;
            }));
        }
    }

    public class PhraseSketchField : VisualElement
    {
        public string Value { get; set; }

        public PhraseSketchField(string value = null, Action<string> onTextChange = null)
        {
            var titleTextField = new TextField
            {
                value = value
            };

            titleTextField.RegisterValueChangedCallback(evt =>
            {
                Value = evt.newValue;
                onTextChange?.Invoke(Value);
            });
            contentContainer.Add(titleTextField);

            onTextChange?.Invoke(value);
        }
    }

    public class NodeTitleErrorField : VisualElement
    {
        public Label Label { get; }

        public NodeTitleErrorField()
        {
            Label = new Label();
            contentContainer.Add(Label);
        }
    }

    #region Choce Cases

    public class ChoicePopupField : VisualElement
    {
        public string Value { get; private set; }

        public ChoicePopupField(List<string> keys)
        {
            var label = new Label();

            contentContainer.Add(new NoEnumPopup(keys, val => label.text = KeyToTextTitle(val)));
            contentContainer.Add(label);
        }

        private string KeyToTextTitle(string val)
        {
            Value = val;
            string textValue = new LocalizedString(val);
            textValue = textValue.Split(" ")[0];
            return textValue;
        }
    }

    public abstract class ChoiceCase : VisualElement
    {
        protected List<string> _choiceKeys;

        public ChoiceCase(string caseName, Action<ChoiceCase> onDelete, List<string> choiceKeys)
        {
            _choiceKeys = choiceKeys;

            contentContainer.style.flexDirection = FlexDirection.Row;

            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = "x",
            });
            contentContainer.Add(new Label(caseName));

            contentContainer.Add(new ChoicePopupField(_choiceKeys));

            contentContainer.Add(new Button(AddCaseField)
            {
                text = "or",
            });
        }

        /// <summary>
        /// Return list of cases that can be one of a case 
        /// </summary>
        /// <returns></returns>
        public List<string> GetOrCases()
        {
            var popups = contentContainer.Query<ChoicePopupField>().ToList();
            var cases = popups.Select(c => c.Value).ToList();
            return cases;
        }
        
        private void AddCaseField() =>
            contentContainer.Add(new ChoicePopupField(_choiceKeys));
    }

    /// <summary>
    /// To easily find data for save/load
    /// </summary>
    public class AndChoiceCase : ChoiceCase
    {
        public AndChoiceCase(string caseName, Action<ChoiceCase> onDelete, List<string> choiceKeys) : base(caseName,
            onDelete, choiceKeys)
        {
        }
    }

    /// <summary>
    /// To easily find data for save/load
    /// </summary>
    public class NoChoiceCase : ChoiceCase
    {
        public NoChoiceCase(string caseName, Action<ChoiceCase> onDelete, List<string> choiceKeys) : base(caseName,
            onDelete, choiceKeys)
        {
        }
    }

    #endregion
}