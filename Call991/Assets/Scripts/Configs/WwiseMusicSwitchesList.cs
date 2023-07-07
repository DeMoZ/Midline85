using System.Collections.Generic;
using AK.Wwise;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/WwiseMusicList", fileName = "MusicList")]
    public class WwiseMusicSwitchesList : ScriptableObject
    {
        [SerializeField] private List<AK.Wwise.Switch> wwiseSwitches = new ();

        public List<Switch> WwiseSwitches => wwiseSwitches;
        
        public virtual List<string> Keys => GetKeys();

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
    }
}