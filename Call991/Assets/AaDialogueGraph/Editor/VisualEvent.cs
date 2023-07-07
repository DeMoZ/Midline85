using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace AaDialogueGraph.Editor
{
    public class VisualEvent : VisualElement
    {
        public PhraseEventType Type;

        public virtual EventVisualData GetData()
        {
            return null;
        }
    }

    public class MusicEventVisual : VisualEvent
    {
        public void Set(EventVisualData data, Action<VisualEvent> onDelete, Action onChange, List<string> musics)
        {
            if (data.Type != PhraseEventType.Music) return;

            Type = data.Type;
            
            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = "X",
            });

            var containerColumn = new VisualElement();
            contentContainer.Add(containerColumn);

            var containerRow1 = new VisualElement();
            containerRow1.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow1);

            musics = musics == null || musics.Count < 1 ? new List<string> { AaGraphConstants.None } : musics;
            var sound = !string.IsNullOrEmpty(data.PhraseEvent)
                ? data.PhraseEvent
                : AaGraphConstants.None;

            var soundPopup = new SoundPopupField(musics, sound)
            {
                name = AaGraphConstants.EventMusicPopupField
            };
            containerRow1.Add(soundPopup);

            var containerRow2 = new VisualElement();
            containerRow2.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow2);

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

        public override EventVisualData GetData()
        {
            return new EventVisualData
            {
                PhraseEvent = contentContainer.Q<SoundPopupField>().Value,
                Type = PhraseEventType.Music,
                Delay = contentContainer.Q<FloatField>().value,
            };
        }
    }
    
    public class RtpcEventVisual : VisualEvent
    {
        public void Set(EventVisualData data, Action<VisualEvent> onDelete, Action onChange, List<string> rtpcs)
        {
            if (data.Type != PhraseEventType.RTPC) return;

            Type = data.Type;
            
            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = "X",
            });

            var containerColumn = new VisualElement();
            contentContainer.Add(containerColumn);

            var containerRow1 = new VisualElement();
            containerRow1.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow1);

            rtpcs = rtpcs == null || rtpcs.Count < 1 ? new List<string> { AaGraphConstants.None } : rtpcs;
            var sound = !string.IsNullOrEmpty(data.PhraseEvent)
                ? data.PhraseEvent
                : AaGraphConstants.None;

            var soundPopup = new SoundPopupField(rtpcs, sound)
            {
                name = AaGraphConstants.EventRtpcPopupField
            };
            containerRow1.Add(soundPopup);

            var containerRow2 = new VisualElement();
            containerRow2.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow2);

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
                name = AaGraphConstants.DelayFieldName,
            };
            delayContainer.Add(delay);

            var volumeContainer = new VisualElement();
            volumeContainer.AddToClassList("aa-Toggle_content-container");
            volumeContainer.style.flexDirection = FlexDirection.Row;
            containerRow2.Add(volumeContainer);
            var volumeLabel = new Label { text = "Value" };
            volumeLabel.tooltip = "RTPC value 0-100, changes sound with effect. More value more effect.";
            volumeContainer.Add(volumeLabel);
            var value = new FloatField
            {
                value = data?.Value ?? 0,
                name = AaGraphConstants.ValueFieldName,
                maxLength = 3,
            };
            volumeContainer.Add(value);
            
            
            
            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.AddToClassList("aa-EventVisual_content-container");
        }

        public override EventVisualData GetData()
        {
            var delay = contentContainer.Q<FloatField>(name: AaGraphConstants.DelayFieldName).value;
            var value = contentContainer.Q<FloatField>(name: AaGraphConstants.ValueFieldName).value;
                
            return new EventVisualData
            {
                PhraseEvent = contentContainer.Q<SoundPopupField>().Value,
                Type = PhraseEventType.RTPC,
                Delay = delay,
                Value = value,
            };
        }
    }
    
    public class SoundEventVisual : VisualEvent
    {
        public void Set(EventVisualData data, Action<VisualEvent> onDelete, Action onChange, List<string> sounds)
        {
            if (data.Type != PhraseEventType.AudioClip) return;

            Type = data.Type;
            
            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = "X",
            });

            var containerColumn = new VisualElement();
            contentContainer.Add(containerColumn);

            var containerRow1 = new VisualElement();
            containerRow1.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow1);

            sounds = sounds == null || sounds.Count < 1 ? new List<string> { AaGraphConstants.None } : sounds;
            var sound = !string.IsNullOrEmpty(data.PhraseEvent)
                ? data.PhraseEvent
                : AaGraphConstants.None;

            var soundPopup = new SoundPopupField(sounds, sound)
            {
                name = AaGraphConstants.EventSoundPopupField
            };
            containerRow1.Add(soundPopup);

            var containerRow2 = new VisualElement();
            containerRow2.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow2);

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

        public override EventVisualData GetData()
        {
            return new EventVisualData
            {
                PhraseEvent = contentContainer.Q<SoundPopupField>().Value,
                Type = PhraseEventType.AudioClip,
                Delay = contentContainer.Q<FloatField>().value,
            };
        }
    }

    public class ObjectEventVisual : VisualEvent
    {
        public void Set(EventVisualData data, Action<VisualEvent> onDelete, Action onChange)
        {
            if (data.Type != PhraseEventType.GameObject && data.Type != PhraseEventType.VideoClip) return;

            Type = data.Type;

            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = "X",
            });

            var containerColumn = new VisualElement();
            contentContainer.Add(containerColumn);

            var containerRow1 = new VisualElement();
            containerRow1.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow1);

            if (data.Type == PhraseEventType.VideoClip)
            {
                var layerOptions = Enum.GetValues(typeof(PhraseEventLayer)).Cast<PhraseEventLayer>().ToList();
                var layerPopup = new PopupField<PhraseEventLayer>("", layerOptions, data?.Layer ?? layerOptions[0],
                    val =>
                    {
                        onChange?.Invoke();
                        return val.ToString();
                    });
                containerRow1.Add(layerPopup);
            }

            var field = new EventAssetField();
            field.Set(data, onChange);
            containerRow1.Add(field);

            var containerRow2 = new VisualElement();
            containerRow2.style.flexDirection = FlexDirection.Row;
            containerColumn.Add(containerRow2);

            if (data.Type == PhraseEventType.VideoClip)
            {
                var loopContainer = new VisualElement();
                loopContainer.AddToClassList("aa-Toggle_content-container");
                containerRow2.Add(loopContainer);

                var loopToggle = new Toggle
                {
                    name = AaGraphConstants.LoopToggleName,
                    text = AaGraphConstants.LoopToggleName,
                    value = data?.Loop ?? false,
                };

                loopContainer.Add(loopToggle);
            }

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

        private EventVisualData GetVideoData()
        {
            var eventAssetField = contentContainer.Q<AssetField>();

            if (eventAssetField == null)
            {
                Debug.LogError("в обьектном эвенте не найдено поле ");
                throw new NullReferenceException();
            }
            else
            {
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

        private EventVisualData GetObjectData()
        {
            var eventAssetField = contentContainer.Q<EventAssetField>();
            return new EventVisualData
            {
                PhraseEvent = eventAssetField.GetEvent(),
                Type = eventAssetField.Type,
                Stop = contentContainer.Q<Toggle>(name: AaGraphConstants.StopToggleName).value,
                Delay = contentContainer.Q<FloatField>().value,
            };
        }

        public override EventVisualData GetData()
        {
            switch (Type)
            {
                case PhraseEventType.VideoClip:
                    return GetVideoData();
                case PhraseEventType.GameObject:
                    return GetObjectData();
                default:
                {
                    Debug.LogError($"error _type = {Type}");
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public class AssetField : VisualElement
    {
        public PhraseEventType Type { get; protected set; }
        public virtual string GetEvent()
        {
            return string.Empty;
        }
    }

    public class EventAssetField : AssetField
    {
        private ObjectField _objectField;

        public void Set(EventVisualData data, Action onChange = null)
        {
            Type = data.Type;

            switch (data.Type)
            {
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

        public override string GetEvent()
        {
            return EditorNodeUtils.GetPathByObject(_objectField.value);
        }
    }
}