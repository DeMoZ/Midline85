using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CountDownView : MonoBehaviour
    {
        public struct Ctx
        {
            public float ChoicesDuration;
        }

        [SerializeField] private Image backLine = default;
        [SerializeField] private Image frontLine = default;
        [SerializeField] private Color startColor = Color.white;
        [SerializeField] private Color endColor = Color.red;
        [SerializeField] private float blendColor = 0.3f;

        private Ctx _ctx;
        private Sequence _sequence;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Stop()
        {
            _sequence.Kill();
        }

        public void Show()
        {
            frontLine.DOColor(startColor, 0);
            backLine.DOColor(startColor, 0);
            
            var sizeDelta = backLine.rectTransform.sizeDelta;
            sizeDelta.x *= 1.5f;
            sizeDelta.y = frontLine.rectTransform.sizeDelta.y;
            frontLine.rectTransform.sizeDelta = sizeDelta;
            gameObject.SetActive(true);
            
            var interval = _ctx.ChoicesDuration * blendColor;
           _sequence = DOTween.Sequence();
            _sequence.Append(frontLine.rectTransform.DOSizeDelta(Vector2.up * frontLine.rectTransform.sizeDelta.y, _ctx.ChoicesDuration));
            _sequence.Insert(interval, frontLine.DOColor(endColor, _ctx.ChoicesDuration * (1 - blendColor)));
            _sequence.Insert(interval, backLine.DOColor(endColor, _ctx.ChoicesDuration * (1 - blendColor)));
        }
    }
}