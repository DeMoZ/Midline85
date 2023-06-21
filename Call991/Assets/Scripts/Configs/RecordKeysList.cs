using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UnityEngine;

namespace Configs
{
    /// <summary>
    /// AaDialogs records list handler. Takes worlds i2loc and pretend that it has it.
    /// </summary>
    [CreateAssetMenu(menuName = "AaDialogueGraph/RecordKeysList")]
    public class RecordKeysList : PopupKeysList
    {
        private const string RecordWordKey = "achive_";
        [SerializeField] private string nope = "No need to add or change anything in that file";
        private string Nope => nope;

        public override List<string> Keys
        {
            get
            {
                keys = LocalizationManager.GetTermsList().Where(cKey => cKey.Contains(RecordWordKey)).ToList();

                return keys;
            }
        }
    }
}