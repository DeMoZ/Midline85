using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class EndNode : AaNode
    {
        public void Set(EndNodeData data, string guid, List<string> sounds, List<string> musics)
        {
            Guid = guid;
            NodeType = AaNodeType.CountNode;
            titleContainer.Add(new EndPopupField(AaKeys.EndKeys, data.End));
            CreateInPort();

            var foldout = new Foldout();
            foldout.value = false;
            foldout.AddToClassList("aa-EndNode_extension-container");
            contentContainer.Add(foldout);

            var events = new AaNodeEvents();
            events.Set(data.EventVisualData, () => { }, sounds, musics);
            foldout.Add(events);

            var recordsContainer = new VisualElement();
            recordsContainer.AddToClassList("aa-RecordVisual_content-container");
            foldout.Add(recordsContainer);
            
            var addRecord = new Button(() =>
            {
                var recordGroup = NewRecordGroup(recordsContainer, foldout, new RecordData());
                recordsContainer.Add(recordGroup);
                UpdateCount(foldout);
            });
            addRecord.text = AaGraphConstants.AddRecord;
            recordsContainer.Add(addRecord);

            foreach (var record in data.Records)
            {
                var recordGroup = NewRecordGroup(recordsContainer, foldout, record);
                recordsContainer.Add(recordGroup);
            }
            
            UpdateCount(foldout);
        }

        private RecordGroup NewRecordGroup(VisualElement container, Foldout foldout, RecordData data)
        {
            var recordGroup =  new RecordGroup();
            recordGroup.Set(element =>
            {
                RemoveElement(element, container);
                UpdateCount(foldout);
            }, AaKeys.RecordKeys, data);

            return recordGroup;
        }

        private void RemoveElement(VisualElement element, VisualElement container)
        {
            container.Remove(element);
        }

        private void UpdateCount(Foldout foldout)
        {
            var records = foldout.Query<RecordGroup>().ToList().Count;
            foldout.text = $"Records {records}";
        }

        public List<RecordData> GetRecords()
        {
            var records = new List<RecordData>();
            var recordGroup = contentContainer.Query<RecordGroup>().ToList();
            foreach (var recordElement in recordGroup)
            {
                var record = new RecordData
                {
                    Key = recordElement.GetKey(),
                    Sprite = recordElement.GetSprite()
                };
                records.Add(record);
            }    
            return records;
        }
    }

    public class RecordGroup : VisualElement
    {
        private SpriteField _objectField;
        
        public void Set(Action<RecordGroup> onDelete, List<string> keys, RecordData data)
        {
            contentContainer.AddToClassList("aa-EndNode_record-container");
            contentContainer.style.flexDirection = FlexDirection.Row;

            var deleteButton = new Button(() => { onDelete?.Invoke(this); });
            deleteButton.text = AaGraphConstants.DeleteNameSmall;
            contentContainer.Add(deleteButton);

            var verticalContainer = new VisualElement();

            var sprite = data != null && !string.IsNullOrEmpty(data.Sprite)
                ? NodeUtils.GetObjectByPath<Sprite>(data.Sprite)
                : null;

            _objectField = new SpriteField();
            _objectField.Set(sprite);

            verticalContainer.Add(_objectField);
            var popupField = new RecordPopupField(keys, data?.Key);
            verticalContainer.Add(popupField);
            contentContainer.Add(verticalContainer);
        }

        public string GetKey()
        {
            var popup = contentContainer.Q<KeyPopupField>();
            var cases = popup.Value;
            return cases;
        }

        public string GetSprite()
        {
            var some = _objectField.GetSprite();
            return EditorNodeUtils.GetPathByObject(_objectField.GetSprite());
        }
    }
}