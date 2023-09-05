using I2.Loc;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenuSettings : AaWindow
    {
        public struct Ctx
        {
            public ReactiveCommand OnClickToMenu;
            public PlayerProfile Profile;
        }

        [SerializeField] private AaMenuButton toMenuTutorialBtn = default;

        [Space] [SerializeField] private LanguageDropdown textLanguage = default;
        [SerializeField] private LanguageDropdown voiceLanguage = default;
        
        [Space] [SerializeField] private AaVolumeSlider masterVolume = default;
        [SerializeField] private AaVolumeSlider voiceVolume = default;
        [SerializeField] private AaVolumeSlider musicVolume = default;
        [SerializeField] private AaVolumeSlider sfxVolume = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            SetTextDropdown();
            SetAudioDropdown();

            masterVolume.Init(_ctx.Profile.OnVolumeSet, _ctx.Profile.MasterVolume);
            voiceVolume.Init(_ctx.Profile.OnVolumeSet, _ctx.Profile.VoiceVolume);
            musicVolume.Init(_ctx.Profile.OnVolumeSet, _ctx.Profile.MusicVolume);
            sfxVolume.Init(_ctx.Profile.OnVolumeSet, _ctx.Profile.SfxVolume);

            toMenuTutorialBtn.onButtonClick.AddListener(OnClickToMenu);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            toMenuTutorialBtn.onButtonClick.RemoveAllListeners();
        }

        public void OnClickToMenu()
        {
            AnimateDisappear(() => { _ctx.OnClickToMenu.Execute(); });
        }

        private void SetTextDropdown()
        {
            var currentLanguage = LocalizationManager.CurrentLanguage;
            if (LocalizationManager.Sources.Count == 0)
                LocalizationManager.UpdateSources();

            var languages = LocalizationManager.GetAllLanguages();

            textLanguage.ClearOptions();
            textLanguage.AddOptions(languages);

            textLanguage.Value(languages.IndexOf(currentLanguage));
            textLanguage.OnValueChanged.RemoveListener(OnTextLanguageSelected);
            textLanguage.OnValueChanged.AddListener(OnTextLanguageSelected);
        }

        private void SetAudioDropdown()
        {
            var currentLanguage = LocalizationManager.CurrentLanguage;
            if (LocalizationManager.Sources.Count == 0)
                LocalizationManager.UpdateSources();

            var languages = LocalizationManager.GetAllLanguages();

            voiceLanguage.ClearOptions();
            voiceLanguage.AddOptions(languages);

            voiceLanguage.Value(1); //languages.IndexOf(currentLanguage);
            voiceLanguage.OnValueChanged.RemoveListener(OnAudioLanguageSelected);
            voiceLanguage.OnValueChanged.AddListener(OnAudioLanguageSelected);
        }

        private void OnAudioLanguageSelected(int index)
        {
        }

        private void OnTextLanguageSelected(int index)
        {
            if (index < 0)
            {
                index = 0;
                textLanguage.Value(index);
            }

            var text = textLanguage.Options[index].text;
            LocalizationManager.CurrentLanguage = text;

            _ctx.Profile.TextLanguage = text;
        }
    }
}