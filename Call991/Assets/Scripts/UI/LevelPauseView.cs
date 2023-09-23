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
            public ReactiveCommand OnClickSkipCinematicButton;
            public ReactiveCommand<bool> OnShowSkipCinematicButton;
        }

        [SerializeField] private AaMenuButton continueButton = default;
        [SerializeField] private AaMenuButton settingsButton = default;
        [SerializeField] private AaMenuButton menuButton = default;
        [SerializeField] private AaMenuButton skipCinematicButton = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.OnShowSkipCinematicButton.Subscribe(OnShowSkipCinematicButton);
            
            continueButton.onButtonClick.AddListener(OnClickContinueHandler);
            settingsButton.onButtonClick.AddListener(OnClickSettingsButtonHandler);
            menuButton.onButtonClick.AddListener(OnClickMenuButtonHandler);
            skipCinematicButton.onButtonClick.AddListener(OnClickSkipCinematicButtonHandler);
        }
        
        private void OnClickContinueHandler() => AnimateDisappear(OnClickContinue);
        private void OnClickSettingsButtonHandler() => AnimateDisappear(OnClickSettingsButton);
        private void OnClickMenuButtonHandler() => AnimateDisappear(OnClickMenuButton);
        private void OnClickSkipCinematicButtonHandler() => AnimateDisappear(OnClickSkipCinematicButton);

        public void OnClickContinue() => _ctx.OnClickUnPauseButton.Execute();
        private void OnClickSettingsButton() => _ctx.OnClickSettingsButton.Execute();
        private void OnClickMenuButton() => _ctx.OnClickMenuButton.Execute();
        private void OnClickSkipCinematicButton() => _ctx.OnClickSkipCinematicButton.Execute();
        
        private void OnShowSkipCinematicButton(bool show) => skipCinematicButton.gameObject.SetActive(show);


        public void Dispose()
        {
            menuButton.onButtonClick.RemoveAllListeners();
            settingsButton.onButtonClick.RemoveAllListeners();
            continueButton.onButtonClick.RemoveAllListeners();
            skipCinematicButton.onButtonClick.RemoveAllListeners();
        }
    }
}