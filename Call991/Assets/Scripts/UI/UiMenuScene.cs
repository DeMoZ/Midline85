using System;
using System.Threading.Tasks;
using Configs;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenuScene : MonoBehaviour, IDisposable
    {
        public struct Ctx
        {
            //public ReactiveCommand OnClickPlayGame;
            public ReactiveCommand OnClickNewGame;
            public PlayerProfile Profile;
            public WwiseAudio AudioManager;
            public GameSet GameSet;
        }

        [SerializeField] private UiMenu menu = default;
        [SerializeField] private UiMenuSettings menuSettings = default;
        [SerializeField] private UiMenuCredits menuCredits = default;
        [SerializeField] private UiMenuSelectLevel menuSelectLevel = default;
        
        [Space] [SerializeField] private AK.Wwise.Switch sceneMusic = default;
        
        private Ctx _ctx;
        private CompositeDisposable _disposables;
        
        private ReactiveCommand _onClickSettings;
        private ReactiveCommand _onClickCredits;
        private ReactiveCommand _onClickToMenu;
        private ReactiveCommand _onClickSelectLevel;

        public async void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            _onClickSettings = new ReactiveCommand();
            _onClickCredits = new ReactiveCommand();
            _onClickToMenu = new ReactiveCommand();
            _onClickSelectLevel = new ReactiveCommand();
            
            _onClickSettings.Subscribe(_ => OnClickSettings()).AddTo(_disposables);
            _onClickCredits.Subscribe(_ => OnClickCredits()).AddTo(_disposables);
            _onClickToMenu.Subscribe(_ => OnClickToMenu()).AddTo(_disposables);
            _onClickSelectLevel.Subscribe(_ => OnClickSelectLevel()).AddTo(_disposables);

            menu.SetCtx(new UiMenu.Ctx
            {
                OnClickSelectLevel = _onClickSelectLevel,
                onClickNewGame = _ctx.OnClickNewGame,
                onClickSettings = _onClickSettings,
                onClickCredits = _onClickCredits,
            });

            menuSelectLevel.SetCtx(new UiMenuSelectLevel.Ctx
            {
                GameSet = _ctx.GameSet,
                Profile = _ctx.Profile,
            });
            
            menuSettings.SetCtx(new UiMenuSettings.Ctx
            {
                onClickToMenu = _onClickToMenu,
                profile = _ctx.Profile,
            });

            menuCredits.SetCtx(new UiMenuCredits.Ctx
            {
                onClickToMenu = _onClickToMenu,
            });

            EnableMenu(menu.GetType());
            await Task.Yield();
            _ctx.AudioManager.PlayMusic(sceneMusic);
        }
        
        private void OnClickSettings() =>
            EnableMenu(menuSettings.GetType());

        private void OnClickCredits() =>
            EnableMenu(menuCredits.GetType());

        private void OnClickToMenu() =>
            EnableMenu(menu.GetType());
        
        private void OnClickSelectLevel()
        {
            menuSelectLevel.Populate();
            EnableMenu(menuSelectLevel.GetType());
        }

        private void EnableMenu(Type type)
        {
            menu.gameObject.SetActive(menu.GetType() == type);
            menuSettings.gameObject.SetActive(menuSettings.GetType() == type);
            menuCredits.gameObject.SetActive(menuCredits.GetType() == type);
            menuSelectLevel.gameObject.SetActive(menuSelectLevel.GetType() == type);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}