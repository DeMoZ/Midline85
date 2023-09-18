using System;
using System.Threading.Tasks;
using Configs;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiOpening : AaWindow
    {
        public struct Ctx
        {
            public GameSet GameSet;
            public ReactiveCommand OnClickStartGame;
            public Blocker Blocker;
            public CursorSet CursorSettings;
            public WwiseAudio AudioManager;
        }

        [SerializeField] private GameObject openingUi1 = default;
        [SerializeField] private GameObject openingUi2 = default;
        [SerializeField] private GameObject openingUi3 = default;
        [SerializeField] private Button startBtn = default;
        [SerializeField] private ClickAnyButton anyButton = default;

        [Space] [SerializeField] private AK.Wwise.Switch sceneMusic = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            startBtn.onClick.AddListener(OnClickStart);
            anyButton.OnClick += OnClickStart;

            openingUi1.gameObject.SetActive(false);
            openingUi2.gameObject.SetActive(false);
            openingUi3.gameObject.SetActive(false);

            OnStateOne();
            _ctx.AudioManager.PlayMusic(sceneMusic);
        }

        public void OnClickStart()
        {
            _ctx.OnClickStartGame.Execute();
        }

        private async void OnStateOne()
        {
            openingUi1.gameObject.SetActive(true);
            openingUi2.gameObject.SetActive(false);
            openingUi3.gameObject.SetActive(false);

            await _ctx.Blocker.FadeScreenBlocker(false, _ctx.GameSet.logoFadeInTime);
            await Task.Delay((int)(_ctx.GameSet.logoHoldTime * 1000));
            await _ctx.Blocker.FadeScreenBlocker(true, _ctx.GameSet.logoFadeOutTime);

            OnStateTwo();
        }

        private async void OnStateTwo()
        {
            openingUi1.gameObject.SetActive(false);
            openingUi2.gameObject.SetActive(true);
            openingUi3.gameObject.SetActive(false);

            await _ctx.Blocker.FadeScreenBlocker(false, _ctx.GameSet.warningFadeInTime);
            await Task.Delay((int)(_ctx.GameSet.warningHoldTime * 1000));
            await _ctx.Blocker.FadeScreenBlocker(true, _ctx.GameSet.warningFadeOutTime);

            OnStateThree();
        }

        private async void OnStateThree()
        {
            openingUi1.gameObject.SetActive(false);
            openingUi2.gameObject.SetActive(false);
            openingUi3.gameObject.SetActive(true);
            _ctx.CursorSettings.ApplyCursor();
            _ctx.CursorSettings.EnableCursor(true);
            await _ctx.Blocker.FadeScreenBlocker(false, _ctx.GameSet.startFadeInTime);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            startBtn.onClick.RemoveAllListeners();
            anyButton.OnClick -= OnClickStart;
        }
    }
}