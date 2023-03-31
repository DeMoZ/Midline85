using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    #region WordCase

    public class ChoicePopupField : VisualElement
    {
        public string Value { get; private set; }

        public ChoicePopupField(List<string> keys, string currentChoice = null)
        {
            var label = new Label();

            contentContainer.Add(new NoEnumPopup(keys, currentChoice, val => label.text = KeyToTextTitle(val)));
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

        public ChoiceCase(string caseName, Action<ChoiceCase> onDelete, List<string> choiceKeys,
            List<string> orCases = null)
        {
            _choiceKeys = choiceKeys;

            contentContainer.style.flexDirection = FlexDirection.Row;

            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = AaGraphConstants.DeleteNameSmall,
            });
            contentContainer.Add(new Label(caseName));

            orCases ??= new List<string> { choiceKeys[0] };

            contentContainer.Add(new ChoicePopupField(_choiceKeys, orCases[0]));

            contentContainer.Add(new Button(() => AddCaseField())
            {
                text = AaGraphConstants.OrName,
            });

            if (orCases.Count > 1)
            {
                for (var i = 1; i < orCases.Count; i++)
                {
                    if (string.IsNullOrEmpty(orCases[i])) orCases[i] = choiceKeys[0];

                    contentContainer.Add(new ChoicePopupField(_choiceKeys, orCases[i]));
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
            contentContainer.Add(new ChoicePopupField(_choiceKeys, currentChoice));
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

    public class EndPopupField : VisualElement
    {
        public string Value { get; private set; }

        public EndPopupField(List<string> keys, string currentChoice = null)
        {
            var label = new Label();

            contentContainer.Add(new NoEnumPopup(keys, currentChoice, val => label.text = KeyToTextTitle(val)));
            contentContainer.Add(label);
        }
        
        private string KeyToTextTitle(string val)
        {
            Value = val;
            return val;
        }
    }

    public abstract class EndCase : VisualElement
    {
        protected List<string> _endKeys;

        public EndCase(string caseName, Action<EndCase> onDelete, List<string> keys, string currentOption = null)
        {
            _endKeys = keys;

            contentContainer.style.flexDirection = FlexDirection.Row;

            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = AaGraphConstants.DeleteNameSmall,
            });
            contentContainer.Add(new Label(caseName));

            contentContainer.Add(new EndPopupField(_endKeys, currentOption));

            contentContainer.Add(new Button(() => AddCaseField())
            {
                text = AaGraphConstants.OrName,
            });
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
            contentContainer.Add(new EndPopupField(_endKeys, currentChoice));
    }

    /// <summary>
    /// To easily find data for save/load
    /// </summary>
    public class AndEndCase : EndCase
    {
        public AndEndCase(string caseName, Action<EndCase> onDelete, List<string> choiceKeys,
            string currentOption = null) : base(caseName, onDelete, choiceKeys, currentOption)
        {
            contentContainer.AddToClassList("aa-ChoiceAsset_content-container-blue");
        }
    }

    /// <summary>
    /// To easily find data for save/load
    /// </summary>
    public class NoEndCase : EndCase
    {
        public NoEndCase(string caseName, Action<EndCase> onDelete, List<string> choiceKeys,
            string currentOption = null) : base(caseName, onDelete, choiceKeys, currentOption)
        {
            contentContainer.AddToClassList("aa-ChoiceAsset_content-container-pink");
        }
    }

    #endregion

    public class CountCase : VisualElement
    {
        public CountCase(Action<CountCase> onDelete, List<string> countKeys, string currentOption = null, int currentValue = 1)
        {
            contentContainer.style.flexDirection = FlexDirection.Row;
            
            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = AaGraphConstants.DeleteNameSmall,
            });

            contentContainer.Add(new Label(AaGraphConstants.Count));
            contentContainer.Add(new EndPopupField(countKeys, currentOption));
            contentContainer.Add(new Label(AaGraphConstants.CountMin));

            IntegerField integerField = new IntegerField();
            integerField.value = currentValue;
            integerField.style.width = 30;
            contentContainer.Add(integerField);
            contentContainer.AddToClassList("aa-ChoiceAsset_content-container-orange");
        }
    }
}