using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    public class MenuButtonAnimation : MonoBehaviour
    {
        [SerializeField] private CanvasGroup background = default;
        [SerializeField] private TMP_Text text = default;

        [Space] [SerializeField] private float duration = 0.5f;
        [SerializeField] private float endPosit = 50f;

        private Sequence _selectSequence;
        private Vector3 _startPos;
        private Vector3 _endPos;

        private void Awake()
        {
            _startPos = text.rectTransform.localPosition;
            _endPos = _startPos;
            _endPos.x += endPosit;
        }

        public void OnSelected()
        {
            Stop();

            _selectSequence.Append(text.rectTransform.DOLocalMoveX(_endPos.x, duration));
            _selectSequence.Insert(0, background.DOFade(1, duration));
        }

        public void OnClick()
        {
        }

        public void OnNormal()
        {
            if (_selectSequence == null) return;

            Stop();
            Normal();
        }

        private void Stop()
        {
            _selectSequence?.Kill();
            _selectSequence = DOTween.Sequence();
            _selectSequence.SetUpdate(true);
        }

        private void Normal()
        {
            _selectSequence.Append(text.rectTransform.DOLocalMoveX(_startPos.x, duration / 2));
            _selectSequence.Insert(0, background.DOFade(0, duration / 2));
        }
    }
}