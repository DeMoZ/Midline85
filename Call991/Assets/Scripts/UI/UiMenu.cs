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

        [SerializeField] private AaMenuButton selectLevelBtn = default;
        [SerializeField] private AaMenuButton newGameBtn = default;
        [SerializeField] private AaMenuButton settingsBtn = default;
        [SerializeField] private AaMenuButton creditsBtn = default;
        [SerializeField] private AaMenuButton exitBtn = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            selectLevelBtn.onButtonClick.AddListener(OnClickSelectLevelHandler);
            selectLevelBtn.onButtonClick.AddListener(OnClickSelectLevelHandler);
            newGameBtn.onButtonClick.AddListener(OnClickNewGameHandler);
            settingsBtn.onButtonClick.AddListener(OnClickSettingsHandler);
            creditsBtn.onButtonClick.AddListener(OnClickCreditsHandler);
            exitBtn.onButtonClick.AddListener(OnClickExitHandler);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            selectLevelBtn.onButtonClick.RemoveAllListeners();
            newGameBtn.onButtonClick.RemoveAllListeners();
            settingsBtn.onButtonClick.RemoveAllListeners();
            creditsBtn.onButtonClick.RemoveAllListeners();
            exitBtn.onButtonClick.RemoveAllListeners();
        }

        private void OnClickSelectLevelHandler() => AnimateDisappear(OnClickSelectLevel);
        private void OnClickNewGameHandler() => AnimateDisappear(OnClickNewGame);
        private void OnClickSettingsHandler() => AnimateDisappear(OnClickSettings);
        private void OnClickCreditsHandler() => AnimateDisappear(OnClickCredits);
        private void OnClickExitHandler() => AnimateDisappear(OnClickExit);

        private void OnClickSelectLevel() => _ctx.OnClickContinue.Execute();
        private void OnClickNewGame() => _ctx.OnClickNewGame.Execute();
        private void OnClickSettings() => _ctx.OnClickSettings.Execute();
        private void OnClickCredits() => _ctx.OnClickCredits.Execute();

        private void OnClickExit() => Application.Quit();
    }
}