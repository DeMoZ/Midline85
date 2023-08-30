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
        private Sequence _sequence;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
           
            continueButton.OnClick += OnClickContinueHandler;
            settingsButton.OnClick += OnClickSettingsButtonHandler;
            menuButton.OnClick += OnClickMenuButtonHandler;
        }

        private void CreateTimeTween(Action callback)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            _sequence.SetUpdate(true);

            var tm = 0f;
            _sequence.Append(DOTween.To(() => tm, x => tm = x, ButtonAnimationTime, ButtonAnimationTime))
                .OnComplete(callback.Invoke);
        }

        private void OnClickContinueHandler() => CreateTimeTween(OnClickContinue);
        private void OnClickSettingsButtonHandler() => CreateTimeTween(OnClickSettingsButton);
        private void OnClickMenuButtonHandler() => CreateTimeTween(OnClickMenuButton);

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