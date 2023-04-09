using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace Configs
{
    /// <summary>
    /// AaDialogs languages list handler. Takes worlds i2loc and pretend that it has it.
    /// </summary>
    [CreateAssetMenu(menuName = "AaDialogueGraph/LanguagesKeysList")]
    public class LanguagesKeysList : PopupKeysList
    {
        [SerializeField] private string nope = "No need to add or change anything in that file";
        private string Nope => nope;
        
        public override List<string> Keys
        {
            get
            {
                // if (!keys.Any())
                // {
                keys = LocalizationManager.GetAllLanguages();
                // }   

                return keys;
            }
        }
    }
}