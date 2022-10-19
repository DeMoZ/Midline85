using System.Linq;
using I2.Loc;
using TMPro;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenuSettings : MonoBehaviour
    {
        public struct Ctx
        {
            public ReactiveCommand onClickToMenu;
            public PlayerProfile profile;
            public AudioManager audioManager;
        }

        [SerializeField] private MenuButtonView toMenuBtn = default;
        [SerializeField] private TMP_InputField inputId = default;
        [SerializeField] private TextMeshProUGUI inputIdText = default;

        [Space] [SerializeField] private LanguageDropdown textLanguage = default;
        [SerializeField] private LanguageDropdown audioLanguage = default;

        private Ctx _ctx;
        private Dialogues _dialogues;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            inputId.text = _ctx.profile.CheatPhrase;
            SetTextDropdown();
            SetAudioDropdown();

            toMenuBtn.OnClick += OnClickToMenu;
            inputId.onValueChanged.AddListener(OnInputId);
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

            _ctx.profile.TextLanguage = text switch
            {
                "English" => Language.EN,
                "Russian" => Language.RU,
                _ => Language.EN
            };
        }

        private void OnClickToMenu()
        {
            _ctx.audioManager.PlayUiSound(SoundUiTypes.MenuButton);
            _ctx.onClickToMenu.Execute();
        }

        private void OnInputId(string value)
        {
            if (_dialogues == null) return;

            var phrase = _dialogues.phrases.FirstOrDefault(p => p.phraseId == value);

            inputIdText.color = phrase == null ? Color.red : Color.blue;
            _ctx.profile.CheatPhrase = phrase == null ? null : phrase.phraseId;
        }

        private async void OnEnable()
        {
            var compositeDialogue = await ResourcesLoader.LoadAsync<ChapterSet>("7_lvl/7_lvl_Total"); // TODO: warning
            _dialogues = await compositeDialogue.LoadDialogues(Language.RU, "7_lvl"); // TODO: warning
        }

        private void OnDisable()
        {
        }
    }
}