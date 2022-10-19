using System;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenuScene : MonoBehaviour, IDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand onClickPlayGame;
            public ReactiveCommand onClickNewGame;
            public PlayerProfile profile;
            public AudioManager audioManager;
        }

        [SerializeField] private UiMenu menu = default;
        [SerializeField] private UiMenuSettings menuSettings = default;

        private Ctx _ctx;
        private CompositeDisposable _disposables;

        public ReactiveCommand _onClickSettings;
        public ReactiveCommand _onClickToMenu;

        public async void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            _onClickSettings = new ReactiveCommand();
            _onClickToMenu = new ReactiveCommand();
            _onClickSettings.Subscribe(_ => OnClickSettings()).AddTo(_disposables);
            _onClickToMenu.Subscribe(_ => OnClickToMenu()).AddTo(_disposables);

            menu.SetCtx(new UiMenu.Ctx
            {
                audioManager = _ctx.audioManager,
                onClickPlayGame = _ctx.onClickPlayGame,
                onClickNewGame = _ctx.onClickNewGame,
                onClickSettings = _onClickSettings
            });

            menuSettings.SetCtx(new UiMenuSettings.Ctx
            {
                audioManager = _ctx.audioManager,
                onClickToMenu = _onClickToMenu,
                profile = _ctx.profile,
            });

            menu.gameObject.SetActive(true);
            menuSettings.gameObject.SetActive(false);
        }

        private void OnClickSettings()
        {
            menu.gameObject.SetActive(false);
            menuSettings.gameObject.SetActive(true);
        }

        private void OnClickToMenu()
        {
            menu.gameObject.SetActive(true);
            menuSettings.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}