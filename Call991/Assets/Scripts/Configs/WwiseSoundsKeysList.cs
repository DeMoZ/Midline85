using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/WwiseSoundsList", fileName = "Sounds_X")]
    public class WwiseSoundsKeysList : ScriptableObject
    {
        [SerializeField] private List<AK.Wwise.Event> wwiseEvents = new ();
        public virtual List<string> Keys => GetKeys();

        [Button("Test See Keys")]
        public List<string> GetKeys()
        {
            var result = new List<string> { AaGraphConstants.None };

            foreach (var evt in wwiseEvents)
            {
                result.Add(evt.Name);
            }

            return result;
        }
    }
}