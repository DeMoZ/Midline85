using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using I2.Loc;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using Button = UnityEngine.UIElements.Button;
using Object = UnityEngine.Object;
using Toggle = UnityEngine.UIElements.Toggle;

namespace AaDialogueGraph.Editor
{
    public static class AaChoices
    {
        private static List<string> _choiceKeys = new();

        public static List<string> ChoiceKeys
        {
            get
            {
                if (!_choiceKeys.Any())
                    _choiceKeys = LocalizationManager.GetTermsList()
                        .Where(cKey => cKey.Contains(AaGraphConstants.CaseWordKey)).ToList();

                return _choiceKeys;
            }
        }
    }

    public static class AaEnds
    {
        private static List<string> _endKeys = new();

        public static List<string> EndKeys
        {
            get
            {
                if (!_endKeys.Any())
                {
                    _endKeys = Resources.Load<GameEnds>("GameEnds").Ends;
                }

                return _endKeys;
            }
        }
    }

    public static class AaGraphConstants
    {
        public const string AssetsResources = "Assets/Resources/";

        public const string DefaultFileName = "NewDialogue";
        public const string NewLanguageName = "new";

        public const string DialogueGraph = "Dialogue Graph";
        public const string NewChoiceNode = "Choices Node";
        public const string NewPhraseNode = "Phrase Node";
        public const string NewForkNode = "Fork Node";

        public const string SaveData = "Save Data";
        public const string LoadData = "Load Data";

        public const string LineSpace = " | ";
        public const string InPortName = "in";
        public const string OutPortName = "out";

        public const string AndWord = "+Word";
        public const string NoWord = "-Word";

        public const string AndEnd = "+End";
        public const string NoEnd = "-End";

        public const string And = "+";
        public const string No = "-";

        public const string HideOnEnd = "HideOnEnd";

        public const string LoopToggleName = "Loop";
        public const string StopToggleName = "Stop";

        public const string DeleteName = "X";
        public const string DeleteNameSmall = "x";
        public const string OrName = "or";

        public const string CaseWordKey = "c.word";
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
            var label = new Label(language);
            label.AddToClassList("aa-BlackText");
            contentContainer.Add(label);
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
            var label = new Label("   Person");
            label.AddToClassList("aa-BlackText");
            contentContainer.Add(label);

            var personContainer = new VisualElement();
            contentContainer.Add(personContainer);

            var personOptions = Enum.GetValues(typeof(Person)).Cast<Person>().ToList();
            var personPopup = new PopupField<Person>("", personOptions, data?.Person ?? personOptions[0],
                (val) => OnPersonChange(val, onPersonChange));
            personContainer.Add(personPopup);

            var positionOptions = Enum.GetValues(typeof(ScreenPlace)).Cast<ScreenPlace>().ToList();
            var positionPopup =
                new PopupField<ScreenPlace>("", positionOptions, data?.ScreenPlace ?? positionOptions[1]);
            personContainer.Add(positionPopup);

            var toggleContainer = new VisualElement();
            var onEndToggle = new Toggle
            {
                text = AaGraphConstants.HideOnEnd,
                value = data?.HideOnEnd ?? false,
            };
            onEndToggle.tooltip = "Person will be hidden after the phrase end";
            toggleContainer.Add(onEndToggle);
            toggleContainer.contentContainer.AddToClassList("aa-Toggle_content-container");
            personContainer.contentContainer.Add(toggleContainer);

            personContainer.style.flexDirection = FlexDirection.Row;

            contentContainer.AddToClassList("aa-PersonVisual_content-container");
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
            var label = new Label("   Phrase");
            label.AddToClassList("aa-BlackText");
            contentContainer.Add(label);

            var phraseContainer = new VisualElement();
            contentContainer.Add(phraseContainer);

            var appearOptions = Enum.GetValues(typeof(TextAppear)).Cast<TextAppear>().ToList();
            var appearPopup = new PopupField<TextAppear>("", appearOptions, data?.TextAppear ?? appearOptions[0]);
            phraseContainer.Add(appearPopup);

            var toggleContainer = new VisualElement();
            var onEndToggle = new Toggle
            {
                text = AaGraphConstants.HideOnEnd,
                value = data?.HideOnEnd ?? false,
            };
            onEndToggle.tooltip = "Phrase will be hidden after the phrase end";
            toggleContainer.contentContainer.AddToClassList("aa-Toggle_content-container");
            phraseContainer.contentContainer.Add(toggleContainer);

            toggleContainer.Add(onEndToggle);

            phraseContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.AddToClassList("aa-PhraseVisual_content-container");
        }

        public PhraseVisualData GetData()
        {
            return new PhraseVisualData
            {
                TextAppear = contentContainer.Q<PopupField<TextAppear>>().value,
                HideOnEnd = contentContainer.Q<Toggle>().value,
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
            var headerContent = new VisualElement();
            headerContent.style.flexDirection = FlexDirection.Row;
            contentContainer.Add(headerContent);

            var label = new Label(text: "Events");
            //label.AddToClassList("aa-BlackText");
            headerContent.Add(label);

            var addSoundEventAssetButton = new Button(() =>
            {
                // add sound
                var eventVisualData = new EventVisualData { Type = PhraseEventType.AudioClip, };
                contentContainer.Add(new EventVisual(eventVisualData, OnDeleteEvent, _onChange));
                _onChange?.Invoke();
            });
            addSoundEventAssetButton.text = "Sound";
            headerContent.Add(addSoundEventAssetButton);

            var addVideoEventAssetButton = new Button(() =>
            {
                // add video
                var eventVisualData = new EventVisualData { Type = PhraseEventType.VideoClip, };
                contentContainer.Add(new EventVisual(eventVisualData, OnDeleteEvent, _onChange));
                _onChange?.Invoke();
            });
            addVideoEventAssetButton.text = "Video";
            headerContent.Add(addVideoEventAssetButton);

            var addObjectEventAssetButton = new Button(() =>
            {
                // add prefab
                var eventVisualData = new EventVisualData { Type = PhraseEventType.GameObject, };
                contentContainer.Add(new EventVisual(eventVisualData, OnDeleteEvent, _onChange));
                _onChange?.Invoke();
            });
            addObjectEventAssetButton.text = "Object";
            headerContent.Add(addObjectEventAssetButton);

            contentContainer.AddToClassList("aa-EventAsset_content-container");

            data?.ForEach(item => contentContainer.Add(new EventVisual(item, OnDeleteEvent, _onChange)));
        }

        private void OnDeleteEvent(EventVisual eventVisual)
        {
            contentContainer.Remove(eventVisual);
            _onChange?.Invoke();
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

            var layerOptions = Enum.GetValues(typeof(PhraseEventLayer)).Cast<PhraseEventLayer>().ToList();
            var layerPopup = new PopupField<PhraseEventLayer>("", layerOptions, data?.Layer ?? layerOptions[0],
                val =>
                {
                    onChange?.Invoke();
                    return val.ToString();
                });
            containerRow1.Add(layerPopup);

            containerRow1.Add(new EventAssetField(data, onChange));

            var containerRow2 = new VisualElement();
            containerRow2.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow2);

            var loopContainer = new VisualElement();
            loopContainer.AddToClassList("aa-Toggle_content-container");
            containerRow2.Add(loopContainer);

            var loopToggle = new Toggle
            {
                name = AaGraphConstants.LoopToggleName,
                text = AaGraphConstants.LoopToggleName,
                value = data?.Loop ?? false,
                //tooltip = "If "
            };
            loopContainer.Add(loopToggle);

            var stopContainer = new VisualElement();
            stopContainer.AddToClassList("aa-Toggle_content-container");
            containerRow2.Add(stopContainer);
            var stopToggle = new Toggle
            {
                name = AaGraphConstants.StopToggleName,
                text = AaGraphConstants.StopToggleName,
                value = data?.Stop ?? false,
                tooltip = "If need to stop the same event started in different phrase node"
            };
            stopContainer.Add(stopToggle);

            var delayContainer = new VisualElement();
            delayContainer.AddToClassList("aa-Toggle_content-container");
            delayContainer.style.flexDirection = FlexDirection.Row;
            containerRow2.Add(delayContainer);
            var delayLabel = new Label { text = "Delay" };
            delayLabel.tooltip = "Time in seconds before the event happen";
            delayContainer.Add(delayLabel);

            var delay = new FloatField
            {
                value = data?.Delay ?? 0,
            };
            delayContainer.Add(delay);

            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.AddToClassList("aa-EventVisual_content-container");
        }

        public EventVisualData GetData()
        {
            var eventAssetField = contentContainer.Q<EventAssetField>();
            return new EventVisualData
            {
                PhraseEvent = eventAssetField.GetEvent(),
                Type = eventAssetField.Type,
                Layer = contentContainer.Q<PopupField<PhraseEventLayer>>().value,
                Loop = contentContainer.Q<Toggle>(name: AaGraphConstants.LoopToggleName).value,
                Stop = contentContainer.Q<Toggle>(name: AaGraphConstants.StopToggleName).value,
                Delay = contentContainer.Q<FloatField>().value,
            };
        }
    }

    public class EventAssetField : VisualElement
    {
        private ObjectField _objectField;
        public PhraseEventType Type { get; }

        public EventAssetField(EventVisualData data, Action onChange = null)
        {
            Type = data.Type;

            switch (data.Type)
            {
                case PhraseEventType.AudioClip:
                    var audioAsset = data.GetEventObject<AudioClip>();
                    _objectField = new ObjectField
                    {
                        objectType = typeof(AudioClip),
                        allowSceneObjects = false,
                        value = audioAsset,
                    };
                    break;
                case PhraseEventType.VideoClip:
                    var videoAsset = data.GetEventObject<VideoClip>();
                    _objectField = new ObjectField
                    {
                        objectType = typeof(VideoClip),
                        allowSceneObjects = false,
                        value = videoAsset,
                    };
                    break;
                case PhraseEventType.GameObject:
                    var objectAsset = data.GetEventObject<GameObject>();
                    _objectField = new ObjectField
                    {
                        objectType = typeof(GameObject),
                        allowSceneObjects = false,
                        value = objectAsset,
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _objectField.RegisterValueChangedCallback(_ => onChange?.Invoke());

            contentContainer.Add(_objectField);
        }

        public string GetEvent()
        {
            return EditorNodeUtils.GetPathByObject(_objectField.value);
        }
    }

    public class _EventAssetField<T> : VisualElement where T : Object
    {
        private ObjectField _objectField;

        public _EventAssetField(T eventAsset = null, Action onChange = null)
        {
            _objectField = new ObjectField
            {
                objectType = typeof(T),
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
        public NoEnumPopup(List<string> keys, string currentChoice = null, Action<string> onChange = null)
        {
            contentContainer.Add(new PopupField<string>("", keys,
                currentChoice ?? keys?[0], val =>
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
}