using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UnityEngine;

namespace Configs
{
    /// <summary>
    /// AaDialogs choice list handler. Takes worlds i2loc.
    /// </summary>
    [CreateAssetMenu(menuName = "AaDialogueGraph/ChoiceKeysList")]
    public class ChoiceKeysList : PopupKeysList
    {
        private const string CaseWordKey = "btn.";
        [SerializeField] private string nope = "No need to add or change anything in that file";
        private string Nope => nope;

        public override List<string> Keys
        {
            get
            {
                keys = LocalizationManager.GetTermsList().Where(cKey => cKey.Contains(CaseWordKey)).ToList();

                return keys;
            }
        }
    }
}