using System.Threading.Tasks;
using Configs;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiOpening : MonoBehaviour
    {
        public struct Ctx
        {
            public GameSet gameSet;
            public ReactiveCommand onClickStartGame;
            public Blocker blocker;
            public CursorSet cursorSettings;
        }

        [SerializeField] private GameObject openingUi1 = default;
        [SerializeField] private GameObject openingUi2 = default;
        [SerializeField] private GameObject openingUi3 = default;
        [SerializeField] private MenuButtonView startBtn = default;
        [SerializeField] private ClickAnyButton anyButton = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            startBtn.OnClick += OnClickStart;
            anyButton.OnClick += OnClickStart;

            openingUi1.gameObject.SetActive(false);
            openingUi2.gameObject.SetActive(false);
            openingUi3.gameObject.SetActive(false);

            OnStateOne();
        }

        public void OnClickStart()
        {
            _ctx.onClickStartGame.Execute();
        }
        
        private async void OnStateOne()
        {
            openingUi1.gameObject.SetActive(true);
            openingUi2.gameObject.SetActive(false);
            openingUi3.gameObject.SetActive(false);
            
            await _ctx.blocker.FadeScreenBlocker(false, _ctx.gameSet.logoFadeInTime);
            await Task.Delay((int) (_ctx.gameSet.logoHoldTime * 1000));
            await _ctx.blocker.FadeScreenBlocker(true, _ctx.gameSet.logoFadeOutTime);
            
            OnStateTwo();
        }

        private async void OnStateTwo()
        {
            openingUi1.gameObject.SetActive(false);
            openingUi2.gameObject.SetActive(true);
            openingUi3.gameObject.SetActive(false);

            await _ctx.blocker.FadeScreenBlocker(false, _ctx.gameSet.warningFadeInTime);
            await Task.Delay((int) (_ctx.gameSet.warningHoldTime * 1000));
            await _ctx.blocker.FadeScreenBlocker(true, _ctx.gameSet.warningFadeOutTime);

            OnStateThree();
        }

        private async void OnStateThree()
        {
            openingUi1.gameObject.SetActive(false);
            openingUi2.gameObject.SetActive(false);
            openingUi3.gameObject.SetActive(true);
            _ctx.cursorSettings.ApplyCursor();
            _ctx.cursorSettings.EnableCursor(true);
            await _ctx.blocker.FadeScreenBlocker(false, _ctx.gameSet.startFadeInTime);
        }

        private void OnDestroy()
        {
            startBtn.OnClick -= OnClickStart;
            anyButton.OnClick -= OnClickStart;
        }
    }
}