using System;
using UniRx;
using UnityEngine;

namespace UI
{
    public class LevelPauseView : AbstractMultiControlComponentsWindow
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
            menuButton.OnClick += () => { _ctx.onClickMenuButton.Execute(); };
            settingsButton.OnClick += () => { _ctx.onClickSettingsButton.Execute(); };
            continueButton.OnClick += OnClickContinue;
        }

        public void OnClickContinue()
        {
            _ctx.onClickUnPauseButton.Execute();
        }
        
        public void Dispose()
        {
            continueButton.OnClick -= OnClickContinue;
        }
    }
}