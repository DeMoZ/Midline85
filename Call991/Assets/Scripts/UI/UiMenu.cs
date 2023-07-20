using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenu : AaWindow
    {
        public struct Ctx
        {
            public ReactiveCommand OnClickContinue;
            public ReactiveCommand OnClickNewGame;
            public ReactiveCommand OnClickSettings;
            public ReactiveCommand OnClickCredits;
        }

        [SerializeField] private MenuButtonView continueBtn = default;
        [SerializeField] private MenuButtonView newGameBtn = default;
        [SerializeField] private MenuButtonView settingsBtn = default;
        [SerializeField] private MenuButtonView creditsBtn = default;
        [SerializeField] private MenuButtonView exitBtn = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            continueBtn.OnClick += OnClickPlay;
            newGameBtn.OnClick += OnClickNewGame;
            settingsBtn.OnClick += OnClickSettings;
            creditsBtn.OnClick += OnClickCredits;
            exitBtn.OnClick += OnClickExit;
        }

        private void OnClickPlay()
        {
            Debug.Log("[UiMenuScene] OnClickContinue");
            _ctx.OnClickContinue.Execute();
        }

        private void OnClickNewGame()
        {
            Debug.Log("[UiMenuScene] OnClickNewGame");
            _ctx.OnClickNewGame.Execute();
        }

        private void OnClickSettings()
        {
            Debug.Log("[UiMenuScene] OnClickSettings");
            _ctx.OnClickSettings.Execute();
        }

        private void OnClickCredits()
        {
            Debug.Log("[UiMenuScene] OnClickCredits");
            _ctx.OnClickCredits.Execute();
        }

        private void OnClickExit()
        {
            Application.Quit();
        }

        public void OnDestroy()
        {
            continueBtn.OnClick -= OnClickPlay;
            newGameBtn.OnClick -= OnClickNewGame;
            settingsBtn.OnClick -= OnClickSettings;
            creditsBtn.OnClick -= OnClickCredits;
            exitBtn.OnClick -= OnClickExit;
        }
    }
}