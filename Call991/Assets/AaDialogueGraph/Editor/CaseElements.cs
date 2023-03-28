using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
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
            string currentOption = null)
        {
            _choiceKeys = choiceKeys;

            contentContainer.style.flexDirection = FlexDirection.Row;

            contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
            {
                text = AaGraphConstants.DeleteNameSmall,
            });
            contentContainer.Add(new Label(caseName));

            contentContainer.Add(new ChoicePopupField(_choiceKeys, currentOption));

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
            var popups = contentContainer.Query<ChoicePopupField>().ToList();
            var cases = popups.Select(c => c.Value).ToList();
            return cases;
        }

        public void AddCaseField(string currentChoice = null) =>
            contentContainer.Add(new ChoicePopupField(_choiceKeys, currentChoice));
    }

    /// <summary>
    /// To easily find data for save/load
    /// </summary>
    public class AndChoiceCase : ChoiceCase
    {
        public AndChoiceCase(string caseName, Action<ChoiceCase> onDelete, List<string> choiceKeys,
            string currentOption = null) : base(caseName, onDelete, choiceKeys, currentOption)
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
            string currentOption = null) : base(caseName, onDelete, choiceKeys, currentOption)
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

        contentContainer.Add(new NoEnumPopup(keys, currentChoice, val => label.text = val));
        contentContainer.Add(label);
    }
}

public abstract class EndCase : VisualElement
{
    protected List<string> _choiceKeys;

    public EndCase(string caseName, Action<EndCase> onDelete, List<string> keys, string currentOption = null)
    {
        _choiceKeys = keys;

        contentContainer.style.flexDirection = FlexDirection.Row;

        contentContainer.Add(new Button(() => { onDelete?.Invoke(this); })
        {
            text = AaGraphConstants.DeleteNameSmall,
        });
        contentContainer.Add(new Label(caseName));

        contentContainer.Add(new EndPopupField(_choiceKeys, currentOption));

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

    public void AddCaseField(string currentChoice = null) =>
        contentContainer.Add(new EndPopupField(_choiceKeys, currentChoice));
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
}