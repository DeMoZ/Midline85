using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
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
    public static class AaKeys
    {
        private static GameSet _gameSet;

        public static List<string> EndKeys => GetGameSet.EndsKeys.Keys;
        public static List<string> CountKeys => GetGameSet.CountKeys.Keys;
        public static List<string> ChoiceKeys => GetGameSet.ChoiceKeys.Keys;
        public static List<string> LanguageKeys => GetGameSet.LanguagesKeys.Keys;
        public static List<string> RecordKeys => GetGameSet.RecordKeys.Keys;
        public static List<string> LevelIdKeys => GetGameSet.LevelKeys.Keys;

        private static GameSet GetGameSet
        {
            get
            {
                if (_gameSet == null)
                {
                    _gameSet = Resources.Load<GameSet>("GameSet");
                }

                return _gameSet;
            }
        }
    }

    public static class AaGraphConstants
    {
        public const string AssetsResources = "Assets/Resources/";

        public const string DefaultFileName = "NewDialogue";

        public const string DialogueGraph = "Dialogue Graph";
        public const string ChoiceNode = "Button";
        public const string PhraseNode = "Phrase";
        public const string ForkNode = "Fork";
        public const string CountNode = "Count";
        public const string EventNode = "Event";
        public const string NewspaperNode = "Newspaper";
        public const string EndNode = "End";

        public const string SaveData = "Save";
        public const string LoadData = "Load";

        public const string LineSpace = " | ";
        public const string InPortName = "in";
        public const string OutPortName = "out";

        public const string AndWord = "+Word";
        public const string NoWord = "-Word";

        public const string AndEnd = "+End";
        public const string NoEnd = "-End";

        public const string AddRecord = "+Record";

        public const string And = "+";
        public const string No = "-";

        public const string Count = "Count";
        public const string PlusCount = "+Count";
        public const string Range = "Range";
        public const string CountMin = "Min";

        public const string HideOnEnd = "HideOnEnd";

        public const string LoopToggleName = "Loop";
        public const string StopToggleName = "Stop";

        public const string DeleteName = "X";
        public const string DeleteNameSmall = "x";
        public const string OrName = "or";
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

    /// <summary>
    /// Helper element to be able to find contaniter with phrases
    /// </summary>
    public class ElementsTable : VisualElement
    {
    }

    public class PhraseElementsRowField : VisualElement
    {
        public void Set(string language, Object clip = null, Phrase phrase = null, Action onChange = null)
        {
            var label = new Label(language);
            label.AddToClassList("aa-BlackText");
            contentContainer.Add(label);
            var soundField = new PhraseSoundField();
            soundField.Set(clip, onChange);
            contentContainer.Add(soundField);

            var assetField = new PhraseAssetField();
            assetField.Set(phrase, onChange);
            contentContainer.Add(assetField);

            contentContainer.style.flexDirection = FlexDirection.Row;
        }
    }
    
    public class NewspaperElementsRowField : VisualElement
    {
        public void Set(string language, Sprite sprite = null, Action onChange = null)
        {
            var label = new Label(language);
            label.AddToClassList("aa-BlackText");
            contentContainer.Add(label);

            var spriteField = new SpriteField();
            spriteField.Set(sprite, onChange);
            contentContainer.Add(spriteField);

            contentContainer.style.flexDirection = FlexDirection.Row;
        }
    }

    public class PhraseSoundField : VisualElement
    {
        private ObjectField _objectField;

        public void Set(Object clip = null, Action onChange = null)
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

        public void Set(Object phraseAsset = null, Action onChange = null)
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

    public class SpriteField : VisualElement
    {
        private ObjectField _objectField;

        public void Set(Object asset = null, Action onChange = null)
        {
            _objectField = new ObjectField
            {
                objectType = typeof(Sprite),
                allowSceneObjects = false,
                value = asset,
            };

            _objectField.RegisterValueChangedCallback(_ => { onChange?.Invoke(); });

            contentContainer.Add(_objectField);
        }

        public Sprite GetSprite()
        {
            return _objectField.value as Sprite;
        }
    }

    public class PersonVisual : VisualElement
    {
        public void Set(PersonVisualData data = null, Action<string> onPersonChange = null)
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
                Person = contentContainer.Q<PopupField<Person>>().value,
                ScreenPlace = contentContainer.Q<PopupField<ScreenPlace>>().value,
                HideOnEnd = contentContainer.Q<Toggle>().value,
            };
        }
    }

    public class PhraseVisual : VisualElement
    {
        public void Set(PhraseVisualData data)
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

    #region NodeEvents

    public class AaNodeEvents : VisualElement
    {
        private Action _onChange;

        public void Set(List<EventVisualData> data, Action onChange)
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
                var eventVisual = new EventVisual();
                eventVisual.Set(eventVisualData, OnDeleteEvent, _onChange);
                contentContainer.Add(eventVisual);
                _onChange?.Invoke();
            });
            addSoundEventAssetButton.text = "Sound";
            headerContent.Add(addSoundEventAssetButton);

            var addVideoEventAssetButton = new Button(() =>
            {
                // add video
                var eventVisualData = new EventVisualData { Type = PhraseEventType.VideoClip, };
                var eventVisual = new EventVisual();
                eventVisual.Set(eventVisualData, OnDeleteEvent, _onChange);
                contentContainer.Add(eventVisual);
                _onChange?.Invoke();
            });
            addVideoEventAssetButton.text = "Video";
            headerContent.Add(addVideoEventAssetButton);

            var addObjectEventAssetButton = new Button(() =>
            {
                // add prefab
                var eventVisualData = new EventVisualData { Type = PhraseEventType.GameObject, };
                var eventVisual = new EventVisual();
                eventVisual.Set(eventVisualData, OnDeleteEvent, _onChange);
                contentContainer.Add(eventVisual);
                _onChange?.Invoke();
            });
            addObjectEventAssetButton.text = "Object";
            headerContent.Add(addObjectEventAssetButton);

            contentContainer.AddToClassList("aa-EventAsset_content-container");

            data?.ForEach(item =>
            {
                var eventVisual = new EventVisual();
                eventVisual.Set(item, OnDeleteEvent, _onChange);
                contentContainer.Add(eventVisual);
            });
        }

        private void OnDeleteEvent(EventVisual eventVisual)
        {
            contentContainer.Remove(eventVisual);
            _onChange?.Invoke();
        }
    }

    public class EventVisual : VisualElement
    {
        public void Set(EventVisualData data, Action<EventVisual> onDelete, Action onChange)
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

            var field = new EventAssetField();
            field.Set(data, onChange);
            containerRow1.Add(field);

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
        public PhraseEventType Type { get; private set; }

        public void Set(EventVisualData data, Action onChange = null)
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

    #endregion

    public class NoEnumPopup : VisualElement
    {
        public void Set(List<string> keys, string currentChoice = null, Action<string> onChange = null)
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

        public void Set(string value = null, Action<string> onTextChange = null)
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
            Set();
        }

        private void Set()
        {
            contentContainer.Add(Label);
        }
    }
}