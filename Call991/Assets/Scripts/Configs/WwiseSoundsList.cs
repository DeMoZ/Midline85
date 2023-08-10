using System.Collections.Generic;
using AK.Wwise;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/WwiseSoundsList", fileName = "SoundsList")]
    public class WwiseSoundsList : ScriptableObject
    {
        [SerializeField] private List<AK.Wwise.Switch> wwiseSwitches = new();

        public List<Switch> WwiseSwitches => wwiseSwitches;

        [Button("Test See Keys")]
        public List<string> GetKeys()
        {
            var result = new List<string> { AaGraphConstants.None };

            foreach (var sw in wwiseSwitches)
            {
                result.Add(sw.Name);
            }

            return result;
        }

        public bool TryGetSwitchByName(string sName, out Switch wSwitch)
        {
            foreach (var sw in wwiseSwitches)
            {
                if(sw.Name.Equals(sName))
                {
                    wSwitch = sw;
                    return true;
                }
            }

            wSwitch = null;
            return false;
        }
    }
}