using I2.Loc;
using TMPro;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenuSettings : AaWindow
    {
        public struct Ctx
        {
            public ReactiveCommand onClickToMenu;
            public PlayerProfile profile;
        }

        [SerializeField] private MenuButtonView toMenuBtn = default;
        [SerializeField] private MenuButtonView toMenuTutorialBtn = default;
        [SerializeField] private TMP_InputField inputId = default;
        [SerializeField] private SettingsVolumeView masterVolume = default;
        [SerializeField] private SettingsVolumeView voiceVolume = default;
        [SerializeField] private SettingsVolumeView musicVolume = default;
        [SerializeField] private SettingsVolumeView sfxVolume = default;

        [Space] [SerializeField] private LanguageDropdown textLanguage = default;
        [SerializeField] private LanguageDropdown audioLanguage = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            inputId.text = "";
            SetTextDropdown();
            SetAudioDropdown();

            masterVolume.Init(_ctx.profile.OnVolumeSet, _ctx.profile.MasterVolume);
            voiceVolume.Init(_ctx.profile.OnVolumeSet, _ctx.profile.VoiceVolume);
            musicVolume.Init(_ctx.profile.OnVolumeSet, _ctx.profile.MusicVolume);
            sfxVolume.Init(_ctx.profile.OnVolumeSet, _ctx.profile.SfxVolume);

            toMenuBtn.OnClick += OnClickToMenu;
            toMenuTutorialBtn.OnClick += OnClickToMenu;
        }

        public void OnClickToMenu()
        {
            _ctx.onClickToMenu.Execute();
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

            audioLanguage.ClearOptions();
            audioLanguage.AddOptions(languages);

            audioLanguage.Value(1); //languages.IndexOf(currentLanguage);
            audioLanguage.OnValueChanged.RemoveListener(OnAudioLanguageSelected);
            audioLanguage.OnValueChanged.AddListener(OnAudioLanguageSelected);
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

            _ctx.profile.TextLanguage = text;
        }
    }
}