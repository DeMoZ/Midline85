using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    /// <summary>
    /// AaDialogs choice list handler. Takes worlds i2loc.
    /// </summary>
    [CreateAssetMenu(menuName = "AaDialogueGraph/"+nameof(FilteredLocalizationKeysList), fileName = nameof(FilteredLocalizationKeysList))]
    public class FilteredLocalizationKeysList : PopupKeysList
    {
        [SerializeField] string FilterKey = "Filter/";
        [SerializeField] private string nope = "No need to add or change anything in that file";
        private string Nope => nope;

        public override List<string> Keys
        {
            get
            {
                keys = LocalizationManager.GetTermsList().Where(cKey => cKey.Contains(FilterKey)).ToList();
                keys.Insert(0,AaGraphConstants.None);
                return keys;
            }
        }
        
        [Button("Test See Keys")][ShowInInspector]
        private List<string> GetKeys()
        {
            return Keys;
        }
    }
}