using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    public class MenuButtonAnimation : MonoBehaviour
    {
        [SerializeField] private RectTransform buttonContent = default;
        [SerializeField] private CanvasGroup background = default;
        [SerializeField] private TMP_Text text = default;

        [Space][SerializeField] private PositionXAnimationConfig selectAnimationConfig = default;
        [SerializeField] private PositionXAnimationConfig clickAnimationConfig = default;

        private Sequence _selectSequence;
        private Sequence _clickSequence;
        private Vector3 _selectStartPos;
        private Vector3 _selectEndPos;

        private void Awake()
        {
            _selectStartPos = text.rectTransform.localPosition;
            _selectEndPos = _selectStartPos;
            _selectEndPos.x += selectAnimationConfig.ToPositionX;
        }

        public void OnSelected()
        {
            Stop();

            _selectSequence.Append(text.rectTransform.DOLocalMoveX(_selectEndPos.x, selectAnimationConfig.AnimationTime));
            _selectSequence.Insert(0, background.DOFade(1, selectAnimationConfig.AnimationTime));
        }

        public void OnClick()
        {
            Stop();
            
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
            _selectSequence.Append(text.rectTransform.DOLocalMoveX(_selectStartPos.x, selectAnimationConfig.AnimationTime / 2));
            _selectSequence.Insert(0, background.DOFade(0, selectAnimationConfig.AnimationTime / 2));
        }
    }
}