using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu]
    public class GameSet : ScriptableObject
    {
        [HideInInspector] public Language textLanguage = Language.EN;
        [HideInInspector] [ReadOnly] public Language audioLanguage = Language.RU;

        [Space] public float choicesDuration = 3f;
        [Space] public float buttonsAppearDuration = 0.2f;
        public float fastButtonFadeDuration = 0.3f;
        public float slowButtonFadeDuration = 0.6f;

        private void OnValidate()
        {
#if UNITY_EDITOR
            var profile = new PlayerProfile();
            profile.SaveLanguages(textLanguage, audioLanguage);
#endif
        }
    }
}