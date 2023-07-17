using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/WwiseVoicesList", fileName = "VoicesList")]
    public class WwiseVoicesList : ScriptableObject
    {
        [SerializeField] private string path;// = "1-Fire/Elena/";
        [SerializeField] private List<AK.Wwise.Event> wwiseEvents;

        //public List<AK.Wwise.Event> WwiseEvents => wwiseEvents;
        public string Path => path;

        [Button("Test See Keys")]
        public List<string> GetKeys()
        {
            var result = new List<string> { AaGraphConstants.None };

            foreach (var rtpc in wwiseEvents)
            {
                result.Add(rtpc.Name);
            }

            return result;
        }
        
        [Button("Test See PathKeys")]
        public List<string> GetPathKeys()
        {
            var result = new List<string> { AaGraphConstants.None };

            foreach (var rtpc in wwiseEvents)
            {
                result.Add($"{path}{rtpc.Name}");
            }

            return result;
        }
        
        public bool TryGetVoiceByName(string sName, out AK.Wwise.Event wEvent)
        {
            foreach (var element in wwiseEvents)
            {
                if (element.Name.Equals(sName))
                {
                    wEvent = element;
                    return true;
                }
            }

            wEvent = null;
            return false;
        }
        
        public bool TryGetVoiceByPath(string fullPath, out AK.Wwise.Event wEvent)
        {
            var sName = fullPath.Replace(path, "");
            foreach (var element in wwiseEvents)
            {
                if (element.Name.Equals(sName))
                {
                    wEvent = element;
                    return true;
                }
            }

            wEvent = null;
            return false;
        }
    }
}