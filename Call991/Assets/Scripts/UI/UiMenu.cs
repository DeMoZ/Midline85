using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenu : AaWindow
    {
        public struct Ctx
        {
            //public ReactiveCommand onClickPlayGame;
            public ReactiveCommand onClickNewGame;
            public ReactiveCommand onClickSettings;
            public ReactiveCommand onClickCredits;
            public ReactiveCommand OnClickSelectLevel;
        }

        [SerializeField] private MenuButtonView playBtn = default;
        [SerializeField] private MenuButtonView newGameBtn = default;
        [SerializeField] private MenuButtonView settingsBtn = default;
        [SerializeField] private MenuButtonView creditsBtn = default;
        [SerializeField] private MenuButtonView exitBtn = default;
        
        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            playBtn.OnClick += OnClickPlay;
            newGameBtn.OnClick += OnClickNewGame;
            settingsBtn.OnClick += OnClickSettings;
            creditsBtn.OnClick += OnClickCredits;
            exitBtn.OnClick += OnClickExit;
        }
        
        private void OnClickPlay()
        {
            Debug.Log("[UiMenuScene] OnClickSelectLevel");
            _ctx.OnClickSelectLevel.Execute();
        }
        
        private void OnClickNewGame()
        {
            Debug.Log("[UiMenuScene] OnClickNewGame");
            _ctx.onClickNewGame.Execute();
        }

        private void OnClickSettings()
        {
            Debug.Log("[UiMenuScene] OnClickSettings");
            _ctx.onClickSettings.Execute();
        }

        private void OnClickCredits()
        {
            Debug.Log("[UiMenuScene] OnClickCredits");
            _ctx.onClickCredits.Execute();
        }

        private void OnClickExit()
        {
            Application.Quit();
        }

        public void OnDestroy()
        {
            playBtn.OnClick -= OnClickPlay;
            newGameBtn.OnClick -= OnClickNewGame;
            settingsBtn.OnClick -= OnClickSettings;
            creditsBtn.OnClick -= OnClickCredits;
            exitBtn.OnClick -= OnClickExit;
        }
    }
}