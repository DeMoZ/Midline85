using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace UI
{
    public class MenuButtonAnimation : MonoBehaviour
    {
        [SerializeField] private bool animateButtonClick;

        [ShowIf("animateButtonClick")] [SerializeField]
        private RectTransform buttonContent = default;

        [ShowIf("animateButtonClick")] [SerializeField]
        private CanvasGroup buttonContentCanvasGroup = default;

        [SerializeField] private CanvasGroup background = default;
        [SerializeField] private TMP_Text text = default;

        [Space] [SerializeField] private PositionXAnimationConfig selectAnimationConfig = default;

        [ShowIf("animateButtonClick")] [SerializeField]
        private PositionXAnimationConfig clickAnimationConfig = default;

        private Sequence _selectSequence;
        private Sequence _clickSequence;

        private Vector3 _selectStartPos;
        private Vector3 _selectEndPos;

        private Vector3 _clickStartPos;
        private Vector3 _clickEndPos;

        private void Awake()
        {
            _selectStartPos = text.rectTransform.localPosition;
            _selectEndPos = _selectStartPos;
            _selectEndPos.x += selectAnimationConfig.ToPositionX;

            if (!animateButtonClick) return;

            _clickStartPos = buttonContent.localPosition;
            _clickEndPos = _clickStartPos;
            _clickEndPos.x += clickAnimationConfig.ToPositionX;
        }

        private void OnEnable()
        {
            if (!animateButtonClick) return;

            buttonContent.localPosition = _clickStartPos;
            buttonContentCanvasGroup.alpha = 1;
        }

        public void OnSelected()
        {
            Stop();

            _selectSequence.Append(
                text.rectTransform.DOLocalMoveX(_selectEndPos.x, selectAnimationConfig.AnimationTime));
            _selectSequence.Insert(0, background.DOFade(1, selectAnimationConfig.AnimationTime));
        }

        public void OnClick()
        {
            if (!animateButtonClick) return;

            _clickSequence?.Kill();
            _clickSequence = DOTween.Sequence();
            _clickSequence.SetUpdate(true);

            _clickSequence.Append(buttonContent.DOLocalMoveX(_clickEndPos.x, clickAnimationConfig.AnimationTime))
                .SetEase(Ease.InQuad);
            _clickSequence.Insert(0, buttonContentCanvasGroup.DOFade(0, clickAnimationConfig.AnimationTime))
                .SetEase(Ease.InQuad);
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
            _selectSequence.Append(text.rectTransform.DOLocalMoveX(_selectStartPos.x,
                selectAnimationConfig.AnimationTime / 2));
            _selectSequence.Insert(0, background.DOFade(0, selectAnimationConfig.AnimationTime / 2));
        }
    }
}