using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu]
    public class GameSet : SerializedScriptableObject
    {
        // [HideInInspector] public Language textLanguage = Language.EN;
        // [HideInInspector] [ReadOnly] public Language audioLanguage = Language.RU;
        [Header("Opening")]
        public float logoHoldTime = 3f;
        public float logoFadeTime = 2f;
        public float warningHoldTime = 5f;
        public float warningFadeTime = 2f;

        [Space] public float choicesDuration = 3f;
        
        [Space] public float buttonsAppearDuration = 0.2f;
        public float fastButtonFadeDuration = 0.3f;
        public float slowButtonFadeDuration = 0.6f;
        
        [Space] public float levelIntroDelay = 2f;
        public float levelEndLevelUiDisappearTime = 3f;
        public float levelEndStatisticsUiFadeTime = 1f;

        [Space]public string titleVideoSoName;
        
        [Space]
        public AudioClip menuBtnClip;
        public AudioClip choiceBtnClip;
        public AudioClip timerClip;
        [Space]
        public Dictionary<string, List<string>> musics;
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            // var profile = new PlayerProfile(new ReactiveCommand<Language>());
            // profile.SaveLanguages(textLanguage, audioLanguage);
#endif
        }
    }
}