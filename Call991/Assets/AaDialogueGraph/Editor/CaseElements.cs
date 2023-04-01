using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public abstract class KeyPopupField : VisualElement
    {
        public string Value { get; private set; }

        protected KeyPopupField(List<string> keys, string currentChoice = null)
        {
            CreateElements(keys, currentChoice);
        }

        protected virtual void CreateElements(List<string> keys, string currentChoice = null)
        {
            var label = new Label();

            contentContainer.Add(new NoEnumPopup(keys, currentChoice, val => label.text = KeyToTextTitle(val)));
            contentContainer.Add(label);
        }

        protected virtual string KeyToTextTitle(string val)
        {
            Value = val;
            return val;
        }
    }

    public class ChoicePopupField : KeyPopupField
    {
        public string Value { get; private set; }

        public ChoicePopupField(List<string> keys, string currentChoice = null) : base(keys, currentChoice)
        {
        }

        protected override string KeyToTextTitle(string val)
        {
            Value = val;
            string textValue = new LocalizedString(val);
            textValue = textValue.Split(" ")[0];
            return textValue;
        }
    }

    public class EndPopupField : KeyPopupField
    {
        public EndPopupField(List<string> keys, string currentChoice = null) : base(keys, currentChoice)
        {
        }

        protected override void CreateElements(List<string> keys, string currentChoice = null)
        {
            contentContainer.Add(new NoEnumPopup(keys, currentChoice, val => KeyToTextTitle(val)));
        }
    }

    public class CountPopupField : KeyPopupField
    {
        public CountPopupField(List<string> keys, string currentChoice = null) : base(keys, currentChoice)
        {
        }

        protected override void CreateElements(List<string> keys, string currentChoice = null)
        {
            contentContainer.Add(new NoEnumPopup(keys, currentChoice, val => KeyToTextTitle(val)));
        }
    }

    #region WordCase

    public abstract class ChoiceCase : VisualElement
    {
        protected List<string> _keys;

        public ChoiceCase(string caseName, Action<ChoiceCase> onDelete, List<string> keys, List<string> orCases = null)
        {
            _keys = keys;

            contentContainer.style.flexDirection = FlexDirection.Row;

            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = AaGraphConstants.DeleteNameSmall,
            });
            contentContainer.Add(new Label(caseName));

            orCases ??= new List<string> { keys[0] };
            contentContainer.Add(new ChoicePopupField(_keys, orCases[0]));

            contentContainer.Add(new Button(() => AddCaseField())
            {
                text = AaGraphConstants.OrName,
            });

            if (orCases.Count > 1)
            {
                for (var i = 1; i < orCases.Count; i++)
                {
                    if (string.IsNullOrEmpty(orCases[i])) orCases[i] = keys[0];

                    contentContainer.Add(new ChoicePopupField(_keys, orCases[i]));
                }
            }
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

        private void AddCaseField(string currentChoice = null) =>
            contentContainer.Add(new ChoicePopupField(_keys, currentChoice));
    }

    /// <summary>
    /// To easily find data for save/load
    /// </summary>
    public class AndChoiceCase : ChoiceCase
    {
        public AndChoiceCase(string caseName, Action<ChoiceCase> onDelete, List<string> choiceKeys,
            List<string> orCases = null) : base(caseName, onDelete, choiceKeys, orCases)
        {
            contentContainer.AddToClassList("aa-ChoiceAsset_content-container-green");
        }
    }

    /// <summary>
    /// To easily find data for save/load
    /// </summary>
    public class NoChoiceCase : ChoiceCase
    {
        public NoChoiceCase(string caseName, Action<ChoiceCase> onDelete, List<string> choiceKeys,
            List<string> orCases = null) : base(caseName, onDelete, choiceKeys, orCases)
        {
            contentContainer.AddToClassList("aa-ChoiceAsset_content-container-red");
        }
    }

    #endregion

    #region End Case

    public abstract class EndCase : VisualElement
    {
        protected List<string> _keys;

        public EndCase(string caseName, Action<EndCase> onDelete, List<string> keys, List<string> orCases = null)
        {
            _keys = keys;

            contentContainer.style.flexDirection = FlexDirection.Row;

            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = AaGraphConstants.DeleteNameSmall,
            });
            contentContainer.Add(new Label(caseName));

            orCases ??= new List<string> { keys[0] };
            contentContainer.Add(new EndPopupField(_keys, orCases[0]));

            contentContainer.Add(new Button(() => AddCaseField())
            {
                text = AaGraphConstants.OrName,
            });

            if (orCases.Count > 1)
            {
                for (var i = 1; i < orCases.Count; i++)
                {
                    if (string.IsNullOrEmpty(orCases[i])) orCases[i] = keys[0];

                    contentContainer.Add(new EndPopupField(_keys, orCases[i]));
                }
            }
        }

        /// <summary>
        /// Return list of cases that can be one of a case 
        /// </summary>
        /// <returns></returns>
        public List<string> GetOrCases()
        {
            var popups = contentContainer.Query<EndPopupField>().ToList();
            var cases = popups.Select(c => c.Value).ToList();
            return cases;
        }

        private void AddCaseField(string currentChoice = null) =>
            contentContainer.Add(new EndPopupField(_keys, currentChoice));
    }

    /// <summary>
    /// To easily find data for save/load
    /// </summary>
    public class AndEndCase : EndCase
    {
        public AndEndCase(string caseName, Action<EndCase> onDelete, List<string> keys,
            List<string> orCases = null) : base(caseName, onDelete, keys, orCases)
        {
            contentContainer.AddToClassList("aa-ChoiceAsset_content-container-blue");
        }
    }

    /// <summary>
    /// To easily find data for save/load
    /// </summary>
    public class NoEndCase : EndCase
    {
        public NoEndCase(string caseName, Action<EndCase> onDelete, List<string> keys,
            List<string> orCases = null) : base(caseName, onDelete, keys, orCases)
        {
            contentContainer.AddToClassList("aa-ChoiceAsset_content-container-pink");
        }
    }

    #endregion

    public class CountCase : VisualElement
    {
        public CountCase(Action<CountCase> onDelete, List<string> countKeys, string currentOption = null,
            Vector2Int? currentValue  = null)
        {
            contentContainer.style.flexDirection = FlexDirection.Row;

            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = AaGraphConstants.DeleteNameSmall,
            });

            contentContainer.Add(new Label(AaGraphConstants.Range));

            var countPopupField = new CountPopupField(countKeys, currentOption);
            contentContainer.Add(countPopupField);

            var rangeField = new Vector2IntField();
            
            if (currentValue.HasValue)
            {
                rangeField.value = currentValue.Value;
            }
            countPopupField.Add(rangeField);
            
            contentContainer.AddToClassList("aa-ChoiceAsset_content-container-orange");
        }

        public string GetKey()
        {
            var popup = contentContainer.Q<CountPopupField>();
            var cases = popup.Value;
            return cases;
        }

        public Vector2Int GetRange()
        {
            var field = contentContainer.Q<Vector2IntField>();
            return field.value;
        }
    }
}