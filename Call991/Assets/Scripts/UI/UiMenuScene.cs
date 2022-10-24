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
        [SerializeField] private UiMenuCredits menuCredits = default;

        private Ctx _ctx;
        private CompositeDisposable _disposables;

        public ReactiveCommand _onClickSettings;
        public ReactiveCommand _onClickCredits;
        public ReactiveCommand _onClickToMenu;

        public async void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            _onClickSettings = new ReactiveCommand();
            _onClickCredits = new ReactiveCommand();
            _onClickToMenu = new ReactiveCommand();
            _onClickSettings.Subscribe(_ => OnClickSettings()).AddTo(_disposables);
            _onClickCredits.Subscribe(_ => OnClickCredits()).AddTo(_disposables);
            _onClickToMenu.Subscribe(_ => OnClickToMenu()).AddTo(_disposables);

            menu.SetCtx(new UiMenu.Ctx
            {
                audioManager = _ctx.audioManager,
                onClickPlayGame = _ctx.onClickPlayGame,
                onClickNewGame = _ctx.onClickNewGame,
                onClickSettings = _onClickSettings,
                onClickCredits = _onClickCredits,
            });

            menuSettings.SetCtx(new UiMenuSettings.Ctx
            {
                audioManager = _ctx.audioManager,
                onClickToMenu = _onClickToMenu,
                profile = _ctx.profile,
            });

            menuCredits.SetCtx(new UiMenuCredits.Ctx
            {
                onClickToMenu = _onClickToMenu,
            });

            EnableMenu(menu.GetType());
        }

        private void OnClickSettings() =>
            EnableMenu(menuSettings.GetType());

        private void OnClickCredits() =>
            EnableMenu(menuCredits.GetType());

        private void OnClickToMenu() =>
            EnableMenu(menu.GetType());

        private void EnableMenu(Type type)
        {
            menu.gameObject.SetActive(menu.GetType() == type);
            menuSettings.gameObject.SetActive(menuSettings.GetType() == type);
            menuCredits.gameObject.SetActive(menuCredits.GetType() == type);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}