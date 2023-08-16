using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/" + nameof(WwiseSoundsSet), fileName = nameof(WwiseSoundsSet))]
    public class WwiseSoundsSet : ScriptableObject
    {
        [SerializeField] private List<LevelSoundsSet> levelSoundsSet = new();

        [Serializable]
        public class LevelSoundsSet
        {
            public WwiseSoundsList SoundsList;
        }

        public List<string> GetKeys()
        {
            var sounds = new List<string>();

            foreach (var set in levelSoundsSet)
            {
                sounds.AddRange(set.SoundsList.GetPathKeys());
            }

            return sounds;
        }

        public string GetSoundByPath(string pathSound)
        {
            var sound = pathSound.Split("/");
            if (sound.Any())
                return sound[^1];

            Debug.LogError($"Sound with pathSound = {pathSound} wasn't found");
            return "NONE";
        }
    }
}