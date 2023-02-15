using UniRx;
using UnityEngine;

namespace UI
{
    public class LevelPauseView : AaWindow
    {
        public struct Ctx
        {
            public ReactiveCommand onClickMenuButton;
            public ReactiveCommand onClickSettingsButton;
            public ReactiveCommand onClickUnPauseButton;
        }

        [SerializeField] private MenuButtonView continueButton = default;
        [SerializeField] private MenuButtonView settingsButton = default;
        [SerializeField] private MenuButtonView menuButton = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            menuButton.OnClick += OnClickMenuButton;
            settingsButton.OnClick += OnClickSettingsButton;
            continueButton.OnClick += OnClickContinue;
        }

        private void OnClickSettingsButton() => 
            _ctx.onClickSettingsButton.Execute();

        private void OnClickMenuButton() => 
            _ctx.onClickMenuButton.Execute();

        public void OnClickContinue() => 
            _ctx.onClickUnPauseButton.Execute();

        public void Dispose()
        {
            menuButton.OnClick -= OnClickMenuButton;
            settingsButton.OnClick -= OnClickSettingsButton;
            continueButton.OnClick -= OnClickContinue;
        }
    }
}