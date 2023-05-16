using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu]
    public class GameSet : SerializedScriptableObject
    {
        [Title("Opening Logo")]
        public float logoFadeInTime = 1f;
        public float logoHoldTime = 3f;
        public float logoFadeOutTime = 2f;
        
        [Title("Opening Warning")]
        public float warningFadeInTime = 1f;
        public float warningHoldTime = 5f;
        public float warningFadeOutTime = 2f;
        
        [Title("Opening Start Game")]
        public float startFadeInTime = 1f;
        [Tooltip("appear delay for lines")]
        public float openingLineAppearTime = 1f;
        [Tooltip("warning before level load")]
        public float startGameOpeningHoldTime = 2f;

        [Title("Level Buttons")]
        public float choicesDuration = 3f;
        
        [Space]
        public float buttonsAppearDuration = 0.2f;
        public float fastButtonFadeDuration = 0.3f;
        public float slowButtonFadeDuration = 0.6f;
        
        [Title("Level Intro")]
        public float levelIntroDelay = 2f;
        public float levelEndLevelUiDisappearTime = 3f;
        public float levelEndStatisticsUiFadeTime = 1f;
        public float newspaperFadeTime = 0.5f;

        [Space][Title("InteractiveVideoRef")]
        public string interactiveVideoRef;

        [Space][Title("ScriptableObject with all the levels")]
        public GameLevelsSo GameLevels;
        
        [Space][Title("DialogueGraph settings")]
        public PopupKeysList CountKeys;
        public PopupKeysList EndsKeys;
        public ChoiceKeysList ChoiceKeys;
        public RecordKeysList RecordKeys;
        public LanguagesKeysList LanguagesKeys;
        public LevelKeysList LevelKeys;
        
        [Space][Title("Button Sounds")]
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