using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/WwiseVoicesSet", fileName = "VoicesSet")]
    public class WwiseVoicesSet : ScriptableObject
    {
        [SerializeField] private List<LevelVoicesSet> levelVoicesSet = new ();

        [Serializable]
        public class LevelVoicesSet
        {
            public WwiseVoicesList VoicesList;
        }

        public List<string> GetKeys()
        {
            var voices = new List<string>();
            
            foreach (var set in levelVoicesSet)
            {
                voices.AddRange(set.VoicesList.GetPathKeys());    
            }

            return voices;
        }

        public string GetVoiceByPath(string pathVoice)
        {
            var voice = pathVoice.Split("/");
            if (voice.Any())
                return voice[^1];

            Debug.LogError($"Voice with pathVoice = {pathVoice} wasnt found");
            return "NONE";
        }
    }
}