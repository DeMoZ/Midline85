using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiCheats : MonoBehaviour
    {
        public struct Ctx
        {
            public ReactiveCommand onClickToMenu;
            public PlayerProfile profile;
        }

        [SerializeField] private MenuButtonView toMenuBtn = default;
        [SerializeField] private TMP_InputField inputId = default;
        [SerializeField] private TextMeshProUGUI inputIdText = default;

        private Ctx _ctx;
        private Dialogues _dialogues;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            inputId.text = _ctx.profile.CheatPhrase;
            toMenuBtn.OnClick += OnClickToMenu;
            inputId.onValueChanged.AddListener(OnInputId);
        }

        private void OnClickToMenu()
        {
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
            var compositeDialogue = await ResourcesLoader.LoadAsync<CompositeDialogue>("7_lvl_Total"); // TODO warning
            _dialogues = compositeDialogue.Load();
        }

        private void OnDisable()
        {
        }
    }
}