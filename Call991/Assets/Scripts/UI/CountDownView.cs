using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class CountDownView : MonoBehaviour
    {
        public struct Ctx
        {
            public float buttonsAppearDuration;
        }

        [SerializeField] private RectTransform backLine = default;
        [SerializeField] private RectTransform frontLine = default;
        [SerializeField] private CanvasGroup canvasGroup = default;

        private Ctx _ctx;
        private Tween _scaleTween;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Stop(float fadeTime)
        {
            _scaleTween.Kill();
            canvasGroup.DOFade(0, fadeTime);
        }

        public async void Show(float time)
        {
            frontLine.sizeDelta = backLine.sizeDelta;
            canvasGroup.alpha = 1;
            gameObject.SetActive(true);
            canvasGroup.DOFade(1, _ctx.buttonsAppearDuration);
            await Task.Delay((int) (_ctx.buttonsAppearDuration * 1000));
            _scaleTween = frontLine.DOSizeDelta(Vector2.up, time).OnUpdate(
                () => { } //OnTweenUpdate
            );
        }

        private void OnTweenUpdate()
        {
            Debug.Log($"{_scaleTween.Elapsed()}");
        }
    }
}