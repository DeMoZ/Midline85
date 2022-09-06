using System.Collections;
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

        private bool _isRunning;
        private float _time;
        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Stop(float fadeTime)
        {
            _isRunning = false;
            canvasGroup.DOFade(0, fadeTime);
        }

        public async void Show(float time)
        {
            _time = time;
            _isRunning = true;
            
            frontLine. sizeDelta = backLine.sizeDelta;
            canvasGroup.alpha = 1;
            gameObject.SetActive(true);
            canvasGroup.DOFade(1, _ctx.buttonsAppearDuration);
            await Task.Delay((int) (_ctx.buttonsAppearDuration * 1000));
            StartCoroutine(TimerRoutine());
        }

        private IEnumerator TimerRoutine()
        {
            var time = _time;
            var sizeDelta = backLine.sizeDelta;

            while (_isRunning)
            {
                yield return null;
                time -= Time.deltaTime;
                sizeDelta.x = backLine.sizeDelta.x * (time / _time);
                frontLine.sizeDelta = sizeDelta;
            }
        }
    }
}