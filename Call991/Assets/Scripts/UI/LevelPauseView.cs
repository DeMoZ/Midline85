using System;
using DG.Tweening;
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

        [SerializeField] private MenuButtonView continueButton = default;
        [SerializeField] private MenuButtonView settingsButton = default;
        [SerializeField] private MenuButtonView menuButton = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
           
            continueButton.OnClick += OnClickContinueHandler;
            settingsButton.OnClick += OnClickSettingsButtonHandler;
            menuButton.OnClick += OnClickMenuButtonHandler;
        }

        private void OnClickContinueHandler() => AnimateDisappear(OnClickContinue);
        private void OnClickSettingsButtonHandler() => AnimateDisappear(OnClickSettingsButton);
        private void OnClickMenuButtonHandler() => AnimateDisappear(OnClickMenuButton);

        public void OnClickContinue() => _ctx.OnClickUnPauseButton.Execute();
        private void OnClickSettingsButton() => _ctx.OnClickSettingsButton.Execute();
        private void OnClickMenuButton() =>_ctx.OnClickMenuButton.Execute();

        public void Dispose()
        {
            menuButton.OnClick -= OnClickMenuButtonHandler;
            settingsButton.OnClick -= OnClickSettingsButtonHandler;
            continueButton.OnClick -= OnClickContinueHandler;
        }
    }
}