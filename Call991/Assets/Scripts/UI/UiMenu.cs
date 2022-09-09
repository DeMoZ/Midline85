using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenu : MonoBehaviour
    {
        public struct Ctx
        {
            public ReactiveCommand onClickPlayGame;
            public ReactiveCommand onClickNewGame;
            public ReactiveCommand onClickSettings;
        }

        [SerializeField] private MenuButtonView playBtn = default;
        [SerializeField] private MenuButtonView newGameBtn = default;
        [SerializeField] private MenuButtonView settingsBtn = default;
        [SerializeField] private MenuButtonView exitBtn = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            playBtn.OnClick += OnClickPlay;
            newGameBtn.OnClick += OnClickNewGame;
            settingsBtn.OnClick += OnClickSettings;
            exitBtn.OnClick += OnClickExit;
        }

        private void OnClickPlay()
        {
            Debug.Log("[UiMenuScene] OnClickPlay");
            _ctx.onClickPlayGame.Execute();
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

        private void OnClickExit()
        {
            Application.Quit();
        }
        
        public void Dispose()
        {
            playBtn.OnClick -= OnClickPlay;
            newGameBtn.OnClick -= OnClickNewGame;
            settingsBtn.OnClick -= OnClickSettings;
            exitBtn.OnClick -= OnClickExit;
        }
    }
}