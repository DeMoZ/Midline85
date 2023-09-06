using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
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
        public static PersonKeysList PersonsKeys => GetGameSet.PersonsKeys;

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
        public void Set(string language, Phrase phrase, Action onChange)
        {
            var label = new Label(language);
            label.AddToClassList("aa-BlackText");
            contentContainer.Add(label);

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

    public class SoundAssetField : VisualElement
    {
        private ObjectField _objectField;

        public void Set(Object phraseAsset, Action onChange = null)
        {
            _objectField = new ObjectField
            {
                objectType = typeof(WwiseSoundsKeysList),
                allowSceneObjects = false,
                value = phraseAsset,
            };

            _objectField.RegisterValueChangedCallback(_ => { onChange?.Invoke(); });

            contentContainer.Add(_objectField);
            contentContainer.AddToClassList("aa-SoundAsset_content-container");
        }

        public WwiseSoundsKeysList GetSoundAsset()
        {
            if (_objectField == null || _objectField.value == null)
                return null;

            return _objectField.value as WwiseSoundsKeysList;
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

            var currentChoice = data?.Person ?? AaKeys.PersonsKeys.Keys[0];
            var personPopup = new PersonPopupField(AaKeys.PersonsKeys.Keys, currentChoice, onPersonChange);
            onPersonChange?.Invoke(currentChoice);
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
                tooltip = "Person will be hidden after the phrase end",
            };
            toggleContainer.Add(onEndToggle);
            toggleContainer.contentContainer.AddToClassList("aa-Toggle_content-container");
            personContainer.contentContainer.Add(toggleContainer);

            personContainer.style.flexDirection = FlexDirection.Row;

            contentContainer.AddToClassList("aa-PersonVisual_content-container");
        }

        public PersonVisualData GetData()
        {
            return new PersonVisualData
            {
                Person = contentContainer.Q<PersonPopupField>().Value,
                ScreenPlace = contentContainer.Q<PopupField<ScreenPlace>>().value,
                HideOnEnd = contentContainer.Q<Toggle>().value,
            };
        }
    }

    public class AaToggle : Toggle
    {
        public AaToggle()
        {
            contentContainer.contentContainer.AddToClassList("aa-Toggle_content-container");
        }
    }

    public class ImagePersonVisual : VisualElement
    {
        public void Set(ImagePersonVisualData data = null, Action<string> onPersonChange = null)
        {
            var label = new Label("   Person");
            label.AddToClassList("aa-BlackText");
            contentContainer.Add(label);

            var currentChoice = data?.Person ?? AaKeys.PersonsKeys.Keys[0];
            var personPopup = new PersonPopupField(AaKeys.PersonsKeys.Keys, currentChoice, onPersonChange);
            onPersonChange?.Invoke(currentChoice);

            var positionOptions =
                Enum.GetValues(typeof(PersonImageScreenPlace)).Cast<PersonImageScreenPlace>().ToList();
            var positionPopup =
                new PopupField<PersonImageScreenPlace>("", positionOptions, data?.ScreenPlace ?? positionOptions[1]);

            var sprite = data is { Sprite: not null } ? NodeUtils.GetObjectByPath<Sprite>(data.Sprite) : null;
            var spriteField = new SpriteField();
            spriteField.Set(sprite, () => { });

            var row1 = new LineGroup(new VisualElement[] { personPopup, positionPopup, spriteField });
            contentContainer.Add(row1);

            var show = new AaToggle
            {
                tooltip = "Person will be shown on phrase start",
                name = AaGraphConstants.ShowOnStart,
                text = AaGraphConstants.ShowOnStart,
                value = data?.ShowOnStart ?? false,
            };

            var hide = new AaToggle
            {
                tooltip = "Person will be hidden after the phrase end",
                name = AaGraphConstants.HideOnEnd,
                text = AaGraphConstants.HideOnEnd,
                value = data?.HideOnEnd ?? false,
            };

            var focus = new AaToggle
            {
                tooltip = "Person will be focused on phrase start",
                name = AaGraphConstants.FocusOnStart,
                text = AaGraphConstants.FocusOnStart,
                value = data?.FocusOnStart ?? false,
            };

            var unfocus = new AaToggle
            {
                tooltip = "Person will be unfocused after the phrase end",
                name = AaGraphConstants.UnfocusOnEnd,
                text = AaGraphConstants.UnfocusOnEnd,
                value = data?.UnfocusOnEnd ?? false,
            };

            var row2 = new LineGroup(new VisualElement[] { show, hide });
            contentContainer.Add(row2);

            var row3 = new LineGroup(new VisualElement[] { focus, unfocus });
            contentContainer.Add(row3);
            contentContainer.AddToClassList("aa-PersonVisual_content-container");
        }

        public ImagePersonVisualData GetData()
        {
            var spite = EditorNodeUtils.GetPathByObject(contentContainer.Q<SpriteField>().GetSprite());

            return new ImagePersonVisualData
            {
                Person = contentContainer.Q<PersonPopupField>().Value,
                ScreenPlace = contentContainer.Q<PopupField<PersonImageScreenPlace>>().value,
                Sprite = spite,
                ShowOnStart = contentContainer.Q<Toggle>(AaGraphConstants.ShowOnStart).value,
                FocusOnStart = contentContainer.Q<Toggle>(AaGraphConstants.FocusOnStart).value,
                UnfocusOnEnd = contentContainer.Q<Toggle>(AaGraphConstants.UnfocusOnEnd).value,
                HideOnEnd = contentContainer.Q<Toggle>(AaGraphConstants.HideOnEnd).value,
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
                value = data?.HideOnEnd ?? true,
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

    public class AaNodeEvents : VisualElement
    {
        private Action _onChange;

        public void Set(List<EventVisualData> data, Action onChange, SoundLists soundLists)
        {
            _onChange = onChange;
            var headerContent = new VisualElement();
            headerContent.style.flexDirection = FlexDirection.Row;
            contentContainer.Add(headerContent);

            var label = new Label(text: "Events");
            headerContent.Add(label);

            var addMusicEventAssetButton = new Button(() =>
            {
                // add music
                var eventVisualData = new EventVisualData { Type = PhraseEventType.Music, };
                var eventVisual = new MusicEventVisual();
                eventVisual.Set(eventVisualData, OnDeleteEvent, _onChange, soundLists.Musics);
                contentContainer.Add(eventVisual);
                _onChange?.Invoke();
            });
            addMusicEventAssetButton.text = PhraseEventType.Music.ToString();

            var addRtpcEventAssetButton = new Button(() =>
            {
                // add RTPC
                var eventVisualData = new EventVisualData { Type = PhraseEventType.RTPC, };
                var eventVisual = new RtpcEventVisual();
                eventVisual.Set(eventVisualData, OnDeleteEvent, _onChange, soundLists.Rtcps);
                contentContainer.Add(eventVisual);
                _onChange?.Invoke();
            });
            addRtpcEventAssetButton.text = PhraseEventType.RTPC.ToString();

            var addSoundEventAssetButton = new Button(() =>
            {
                // add sound
                var eventVisualData = new EventVisualData { Type = PhraseEventType.AudioClip, };
                var eventVisual = new SoundEventVisual();
                eventVisual.Set(eventVisualData, OnDeleteEvent, _onChange, soundLists.Sfxs);
                contentContainer.Add(eventVisual);
                _onChange?.Invoke();
            });
            addSoundEventAssetButton.text = "Sound";

            var addImageEventAssetButton = new Button(() =>
            {
                // add image
                var eventVisualData = new EventVisualData { Type = PhraseEventType.Image, };
                var eventVisual = new ObjectEventVisual();
                eventVisual.Set(eventVisualData, OnDeleteEvent, _onChange, AaGraphConstants.ImageField);
                contentContainer.Add(eventVisual);
                _onChange?.Invoke();
            });
            addImageEventAssetButton.text = AaGraphConstants.ImageField;
            
            var addVideoEventAssetButton = new Button(() =>
            {
                // add video
                var eventVisualData = new EventVisualData { Type = PhraseEventType.VideoClip, };
                var eventVisual = new ObjectEventVisual();
                eventVisual.Set(eventVisualData, OnDeleteEvent, _onChange, AaGraphConstants.VideoField);
                contentContainer.Add(eventVisual);
                _onChange?.Invoke();
            });
            addVideoEventAssetButton.text = AaGraphConstants.VideoField;

            var addObjectEventAssetButton = new Button(() =>
            {
                // add prefab
                var eventVisualData = new EventVisualData { Type = PhraseEventType.GameObject, };
                var eventVisual = new ObjectEventVisual();
                eventVisual.Set(eventVisualData, OnDeleteEvent, _onChange, AaGraphConstants.ObjectField);
                contentContainer.Add(eventVisual);
                _onChange?.Invoke();
            });
            addObjectEventAssetButton.text = AaGraphConstants.ObjectField;
            
            var addProjectorEventAssetButton = new Button(() =>
            {
                // add projector
                var eventVisualData = new EventVisualData { Type = PhraseEventType.Projector, };
                var eventVisual = new ObjectEventVisual();
                eventVisual.Set(eventVisualData, OnDeleteEvent, _onChange, AaGraphConstants.ProjectorField);
                contentContainer.Add(eventVisual);
                _onChange?.Invoke();
            });
            addProjectorEventAssetButton.text = AaGraphConstants.ProjectorField;

            var buttonsGroup = new VisualElement();
            headerContent.Add(buttonsGroup);

            var line1 = new LineGroup(new[]
                { addMusicEventAssetButton, addRtpcEventAssetButton, addSoundEventAssetButton });
            var line2 = new LineGroup(new[]
                { addImageEventAssetButton, addVideoEventAssetButton, addObjectEventAssetButton });
            var line3 = new LineGroup(new[]
                { addProjectorEventAssetButton});
            buttonsGroup.Add(line1);
            buttonsGroup.Add(line2);
            buttonsGroup.Add(line3);

            contentContainer.AddToClassList("aa-EventAsset_content-container");

            data?.ForEach(item =>
            {
                switch (item.Type)
                {
                    case PhraseEventType.Music:
                        var musicEventVisual = new MusicEventVisual();
                        musicEventVisual.Set(item, OnDeleteEvent, _onChange, soundLists.Musics);
                        contentContainer.Add(musicEventVisual);
                        break;

                    case PhraseEventType.RTPC:
                        var rtpcventVisual = new RtpcEventVisual();
                        rtpcventVisual.Set(item, OnDeleteEvent, _onChange, soundLists.Rtcps);
                        contentContainer.Add(rtpcventVisual);
                        break;
                    case PhraseEventType.AudioClip:
                        var soundEventVisual = new SoundEventVisual();
                        soundEventVisual.Set(item, OnDeleteEvent, _onChange, soundLists.Sfxs);
                        contentContainer.Add(soundEventVisual);
                        break;
                    case PhraseEventType.Projector:
                    case PhraseEventType.Image:
                    case PhraseEventType.VideoClip:
                    case PhraseEventType.GameObject:
                        var objectEventVisual = new ObjectEventVisual();
                        objectEventVisual.Set(item, OnDeleteEvent, _onChange, GetEventFieldName(item.Type));
                        contentContainer.Add(objectEventVisual);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        private string GetEventFieldName(PhraseEventType type)
        {
            switch (type)
            {
                case PhraseEventType.Image:
                    return AaGraphConstants.ImageField;
                case PhraseEventType.VideoClip:
                    return AaGraphConstants.VideoField;
                case PhraseEventType.GameObject:
                    return AaGraphConstants.ObjectField;
                case PhraseEventType.Projector:
                    return AaGraphConstants.ProjectorField;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDeleteEvent(VisualEvent eventVisual)
        {
            contentContainer.Remove(eventVisual);
            _onChange?.Invoke();
        }
    }

    public class NoEnumPopup : VisualElement
    {
        public void Set(List<string> keys, string currentChoice = null, Action<string> onChange = null)
        {
            keys = keys == null || keys.Count < 1 ? new List<string> { AaGraphConstants.None } : keys;
            currentChoice = string.IsNullOrEmpty(currentChoice) || !keys.Contains(currentChoice)
                ? keys.Count > 0 ? keys[0] : AaGraphConstants.None
                : currentChoice;

            contentContainer.Add(new PopupField<string>("", keys, currentChoice, val =>
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

    public class LineGroup : VisualElement
    {
        public LineGroup(IEnumerable<VisualElement> elements)
        {
            foreach (var element in elements)
            {
                contentContainer.Add(element);
            }

            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.AddToClassList("aa-LineGroup_content-container");
        }
    }

    public class ButtonFilterTextField : TextField
    {
    }
}