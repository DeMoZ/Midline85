using System.Threading.Tasks;
using Configs;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiOpening : MonoBehaviour
    {
        public struct Ctx
        {
            public GameSet gameSet;
            public AudioManager audioManager;
            public ReactiveCommand onClickStartGame;
        }

        [SerializeField] private CanvasGroup openingUi1 = default;
        [SerializeField] private CanvasGroup openingUi2 = default;
        [SerializeField] private CanvasGroup openingUi3 = default;
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
            openingUi1.alpha = 1;
            openingUi1.gameObject.SetActive(true);
            openingUi2.gameObject.SetActive(false);
            openingUi3.gameObject.SetActive(false);

            await Task.Delay((int) (_ctx.gameSet.logoHoldTime * 1000));
            openingUi1.DOFade(0, _ctx.gameSet.logoFadeTime).OnComplete(OnStateTwo);
        }

        private async void OnStateTwo()
        {
            openingUi2.alpha = 1;
            openingUi1.gameObject.SetActive(false);
            openingUi2.gameObject.SetActive(true);
            openingUi3.gameObject.SetActive(false);

            await Task.Delay((int) (_ctx.gameSet.warningHoldTime * 1000));
            openingUi2.DOFade(0, _ctx.gameSet.warningFadeTime).OnComplete(OnStateThree);
        }

        private void OnStateThree()
        {
            openingUi3.alpha = 1;
            openingUi1.gameObject.SetActive(false);
            openingUi2.gameObject.SetActive(false);
            openingUi3.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            startBtn.OnClick -= OnClickStart;
            anyButton.OnClick -= OnClickStart;
        }
    }
}