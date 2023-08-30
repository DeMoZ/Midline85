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

        [SerializeField] private MenuButtonView selectLevelBtn = default;
        [SerializeField] private MenuButtonView newGameBtn = default;
        [SerializeField] private MenuButtonView settingsBtn = default;
        [SerializeField] private MenuButtonView creditsBtn = default;
        [SerializeField] private MenuButtonView exitBtn = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            selectLevelBtn.OnClick += OnClickSelectLevelHandler;
            newGameBtn.OnClick += OnClickNewGameHandler;
            settingsBtn.OnClick += OnClickSettingsHandler;
            creditsBtn.OnClick += OnClickCreditsHandler;
            exitBtn.OnClick += OnClickExitHandler;
        }

        private void OnClickSelectLevelHandler() => Invoke(nameof(OnClickSelectLevel), ButtonAnimationTime);
        private void OnClickNewGameHandler() => Invoke(nameof(OnClickNewGame), ButtonAnimationTime);
        private void OnClickSettingsHandler() => Invoke(nameof(OnClickSettings), ButtonAnimationTime);
        private void OnClickCreditsHandler() => Invoke(nameof(OnClickCredits), ButtonAnimationTime);
        private void OnClickExitHandler() => Invoke(nameof(OnClickExit), ButtonAnimationTime);

        private void OnClickSelectLevel()
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

        protected override void OnDestroy()
        {
            base.OnDestroy();

            selectLevelBtn.OnClick -= OnClickSelectLevelHandler;
            newGameBtn.OnClick -= OnClickNewGameHandler;
            settingsBtn.OnClick -= OnClickSettingsHandler;
            creditsBtn.OnClick -= OnClickCreditsHandler;
            exitBtn.OnClick -= OnClickExitHandler;
        }
    }
}