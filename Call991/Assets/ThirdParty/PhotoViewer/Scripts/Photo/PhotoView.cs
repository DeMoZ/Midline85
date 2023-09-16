using System;
using DG.Tweening;
using UI;
using UnityEngine;

namespace PhotoViewer.Scripts.Photo
{
    public class PhotoView : AaWindow
    {
        private const float MagicValue = 3000f;

        [SerializeField] private RectTransform viewTransform = default;
        [Space] [SerializeField] private RectTransform backgroundImage = default;
        [SerializeField] private RectTransform newspaperContent = default;
        [Space] [SerializeField] private AaMenuButton closeBtn = default;
        [SerializeField] private NewspaperInput newspaperInput = default;
        [SerializeField] private NewspaperInputSo newspaperInputConfig = default;

        private RectTransform _backgroundTransform;
        private RectTransform _newspaperTransform;

        private bool _zoomIn;
        private Sequence _zoomSequence;
        private Sequence _scrollSequence;
        private bool _isScrolling;

        public event Action OnClose;

        private Vector2 ViewerSize
        {
            get
            {
                var rect = viewTransform.rect;
                return new Vector2(rect.width, rect.height);
            }
        }

        private Vector2 NewspaperSize
        {
            get
            {
                var rect = _newspaperTransform.rect;
                var angle = (int)_newspaperTransform.rotation.eulerAngles.z;
                Vector2 result;

                if (angle == 0 || angle == 180)
                    result = new Vector2(rect.width, rect.height);
                else
                    result = new Vector2(rect.height, rect.width);

                return result;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _backgroundTransform = backgroundImage;
            _newspaperTransform = newspaperContent;

            newspaperInput.Init(newspaperInputConfig);

            //_newspaperInput.onDrag += ApplyMove;
            newspaperInput.onScroll += ApplyScroll;
            newspaperInput.onClick += ZoomOnClick;
            closeBtn.onButtonClick.AddListener(Close);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            //_newspaperInput.onDrag -= ApplyMove;
            newspaperInput.onScroll -= ApplyScroll;
            newspaperInput.onClick -= ZoomOnClick;
            closeBtn.onButtonClick.RemoveAllListeners();
        }

        public void SetNewspaper(GameObject content)
        {
            if (content == null) return;

            Instantiate(content, newspaperContent);
        }

        private void Close()
        {
            OnClose?.Invoke();
        }

        private void ZoomOnClick(Vector2 clickPos)
        {
            _zoomIn = !_zoomIn;

            _zoomSequence?.Kill();
            _scrollSequence?.Kill();

            _zoomSequence = DOTween.Sequence().SetEase(Ease.InOutCubic);
            _zoomSequence.SetUpdate(true);

            _scrollSequence = DOTween.Sequence().SetEase(Ease.InOutCubic);
            _scrollSequence.SetUpdate(true).OnUpdate(() =>
            {
                if (_zoomIn && _isScrolling)
                    _scrollSequence.Kill();
            });

            // newspaperImage
            var zoomSize = _zoomIn ? Vector2.one * newspaperInputConfig.MaxZoom : Vector2.one;
            _zoomSequence.Append(_newspaperTransform.DOScale(zoomSize, newspaperInputConfig.ZoomTime));

            var position = _zoomIn ? CalculateNewspaperZoomPosition(clickPos) : Vector2.zero;
            _scrollSequence.Append(_newspaperTransform.DOLocalMove(position, newspaperInputConfig.ZoomTime));

            // backgroundImage
            var backZoomSize = _zoomIn
                ? Vector2.one + Vector2.one * (newspaperInputConfig.MaxZoom * 0.2f)
                : Vector2.one;
            _zoomSequence.Insert(0, _backgroundTransform.DOScale(backZoomSize, newspaperInputConfig.ZoomTime));

            var backPosition = _zoomIn ? CalculateBackgroundZoomPosition(clickPos) : Vector2.zero;
            _scrollSequence.Insert(0, _backgroundTransform.DOLocalMove(backPosition, newspaperInputConfig.ZoomTime));
        }

        // todo remove hardcode
        private Vector2 CalculateNewspaperZoomPosition(Vector2 clickPos)
        {
            var relation = clickPos.y / Screen.height;
            var coordinate = MagicValue * 0.5f - MagicValue * relation;

            return new Vector2(0, coordinate);
        }

        // todo remove hardcode
        private Vector2 CalculateBackgroundZoomPosition(Vector2 clickPos)
        {
            var magicValue = MagicValue * 0.1f;
            var relation = clickPos.y / Screen.height;
            var coordinate = magicValue * 0.5f - magicValue * relation;

            return new Vector2(0, coordinate);
        }

        private void ApplyScroll(float scrollDelta)
        {
            if (!_zoomIn) return;

            _isScrolling = scrollDelta > 0;
            ApplyNewspaperScroll(scrollDelta);
            ApplyBackgroundScroll(scrollDelta);
        }

        // todo remove hardcode
        private void ApplyNewspaperScroll(float zoomDelta)
        {
            Vector2 newPosition = _newspaperTransform.localPosition;
            newPosition.y += zoomDelta;

            //newPosition.x = Mathf.Clamp(newPosition.x, -1200, 1200);
            var magicValue = MagicValue * 0.5f;
            newPosition.y = Mathf.Clamp(newPosition.y, -magicValue, magicValue);

            _newspaperTransform.localPosition = newPosition;
        }

        // todo remove hardcode
        private void ApplyBackgroundScroll(float zoomDelta)
        {
            Vector2 newPosition = _backgroundTransform.localPosition;
            newPosition.y += zoomDelta * 0.1f;
            var magicValue = MagicValue * 0.05f;
            newPosition.y = Mathf.Clamp(newPosition.y, -magicValue, magicValue);

            _backgroundTransform.localPosition = newPosition;
        }


        /*private void ApplyMove(Vector2 deltaPosition)
        {
            if (!_zoomIn) return;

            Vector2 newPosition = imageTransform.localPosition;
            newPosition += deltaPosition;

            newPosition.x = Mathf.Clamp(newPosition.x, -1200, 1200);
            newPosition.y = Mathf.Clamp(newPosition.y, -1500, 1500);

            imageTransform.localPosition = newPosition;
        }*/
    }
}