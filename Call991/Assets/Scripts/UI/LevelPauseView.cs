using UniRx;
using UnityEngine;

namespace UI
{
    public class LevelPauseView : AaWindow
    {
        public struct Ctx
        {
            public ReactiveCommand OnClickMenuButton;
            public ReactiveCommand OnClickSettingsButton;
            public ReactiveCommand OnClickUnPauseButton;
        }

        [SerializeField] private AaMenuButton continueButton = default;
        [SerializeField] private AaMenuButton settingsButton = default;
        [SerializeField] private AaMenuButton menuButton = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;

            continueButton.onButtonClick.AddListener(OnClickContinueHandler);
            settingsButton.onButtonClick.AddListener(OnClickSettingsButtonHandler);
            menuButton.onButtonClick.AddListener(OnClickMenuButtonHandler);
        }

        private void OnClickContinueHandler() => AnimateDisappear(OnClickContinue);
        private void OnClickSettingsButtonHandler() => AnimateDisappear(OnClickSettingsButton);
        private void OnClickMenuButtonHandler() => AnimateDisappear(OnClickMenuButton);

        public void OnClickContinue() => _ctx.OnClickUnPauseButton.Execute();
        private void OnClickSettingsButton() => _ctx.OnClickSettingsButton.Execute();
        private void OnClickMenuButton() => _ctx.OnClickMenuButton.Execute();

        public void Dispose()
        {
            menuButton.onButtonClick.RemoveAllListeners();
            settingsButton.onButtonClick.RemoveAllListeners();
            continueButton.onButtonClick.RemoveAllListeners();
        }
    }
}