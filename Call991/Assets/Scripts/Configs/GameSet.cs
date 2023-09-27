using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu]
    public class GameSet : SerializedScriptableObject
    {
        [Title("Level Buttons")]
        public float choicesDuration = 3f;
        
        [Space]
        public float buttonsAppearDuration = 0.2f;
        public float buttonsShowSelectionDuration = 0.5f;
        public float buttonsDisappearDuration = 1.2f;
        
        [Title("Level Timings")]
        public float levelIntroDelay = 2f;
        public float levelEndStatisticsUiFadeTime = 1f;
        public float shortFadeTime = 0.5f;
        [Space] public float levelWarningTotalDelay = 4f;
        public float levelWarningLineDelay = 1f;
        public float levelWarningLineFadeTime = 0.5f;
        
        [Space][Title("ScriptableObject with all the levels")]
        public GameLevelsSo GameLevels;

        [Space] [Title("DialogueGraph settings")]
        public WwiseSoundsSet VoicesSet;
        public WwiseSoundsSet SfxsSet;
        public WwiseMusicSwitchesList MusicSwitchesKeys;
        public WwiseRtpcList RtpcKeys;
        public LevelKeysList LevelKeys;
        public PersonKeysList PersonsKeys;
        public PopupKeysList CountKeys;
        public FilteredLocalizationKeysList EndsKeys;
        public ChoiceKeysList ChoiceKeys;
        public RecordKeysList RecordKeys;
        public LanguagesKeysList LanguagesKeys;

        private void OnValidate()
        {
#if UNITY_EDITOR
            // var profile = new PlayerProfile(new ReactiveCommand<Language>());
            // profile.SaveLanguages(textLanguage, audioLanguage);
#endif
        }
    }
}