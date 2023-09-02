using System;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace PhotoViewer.Scripts.Photo
{
    public class PhotoView : AaWindow
    {
        private const float MagicValue = 3000f;
        
        [SerializeField] private RectTransform viewTransform = default;
        [Space] [SerializeField] private Image backgroundImage = default;
        [SerializeField] private Image newspaperImage = default;
        [Space] [SerializeField] private MenuButtonView closeBtn = default;
        [SerializeField] private NewspaperInput newspaperInput = default;
        [SerializeField] private NewspaperInputSo newspaperInputConfig = default;

        private RectTransform _backgroundTransform;
        private RectTransform _newspaperTransform;

        private Vector2 _initialImageSize;
        private bool _zoomIn;
        private Sequence _zoomSequence;

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
            _backgroundTransform = backgroundImage.rectTransform;
            _newspaperTransform = newspaperImage.rectTransform;

            newspaperInput.Init(newspaperInputConfig);

            //_newspaperInput.onDrag += ApplyMove;
            newspaperInput.onScroll += ApplyScroll;
            newspaperInput.onClick += ZoomOnClick;
            closeBtn.OnClick += Close;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            //_newspaperInput.onDrag -= ApplyMove;
            newspaperInput.onScroll -= ApplyScroll;
            newspaperInput.onClick -= ZoomOnClick;
            closeBtn.OnClick -= Close;
        }

        public void SetNewspaper(Sprite sprite)
        {
            var imageData = new ImageData
            {
                Sprite = sprite,
            };

            ShowData(imageData);
        }

        private void Close()
        {
            OnClose?.Invoke();
        }

        private void ShowData(ImageData imageData)
        {
            if (newspaperImage)
                newspaperImage.sprite = imageData.Sprite;

            _initialImageSize = ViewerSize;
        }

        private void ZoomOnClick(Vector2 clickPos)
        {
            _zoomIn = !_zoomIn;

            _zoomSequence?.Kill();
            _zoomSequence = DOTween.Sequence().SetEase(Ease.InOutCubic);
            _zoomSequence.SetUpdate(true);

            // newspaperImage
            var zoomSize = _zoomIn ? _initialImageSize * newspaperInputConfig.MaxZoom : _initialImageSize;
            _zoomSequence.Append(_newspaperTransform.DOSizeDelta(zoomSize, newspaperInputConfig.ZoomTime));

            var position = _zoomIn ? CalculateNewspaperZoomPosition(clickPos) : Vector2.zero;
            _zoomSequence.Insert(0, _newspaperTransform.DOLocalMove(position, newspaperInputConfig.ZoomTime));

            // backgroundImage
            var backZoomSize = _zoomIn ? _initialImageSize * (newspaperInputConfig.MaxZoom * 0.5f) : _initialImageSize;
            _zoomSequence.Insert(0, _backgroundTransform.DOSizeDelta(backZoomSize, newspaperInputConfig.ZoomTime));

            var backPosition = _zoomIn ? CalculateBackgroundZoomPosition(clickPos) : Vector2.zero;
            _zoomSequence.Insert(0, _backgroundTransform.DOLocalMove(backPosition, newspaperInputConfig.ZoomTime));
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

        private void ApplyScroll(float zoomDelta)
        {
            ApplyNewspaperScroll(zoomDelta, _newspaperTransform);
            ApplyBackgroundScroll(zoomDelta, _backgroundTransform);
        }

        // todo remove hardcode
        private void ApplyNewspaperScroll(float zoomDelta, Transform obj)
        {
            if (!_zoomIn) return;

            Vector2 newPosition = obj.localPosition;
            newPosition.y += zoomDelta;

            //newPosition.x = Mathf.Clamp(newPosition.x, -1200, 1200);
            var magicValue = MagicValue * 0.5f;
            newPosition.y = Mathf.Clamp(newPosition.y, -magicValue, magicValue);

            obj.localPosition = newPosition;
        }

        // todo remove hardcode
        private void ApplyBackgroundScroll(float zoomDelta, Transform obj)
        {
            if (!_zoomIn) return;

            Vector2 newPosition = obj.localPosition;
            newPosition.y += zoomDelta * 0.1f;
            var magicValue = MagicValue * 0.05f;
            newPosition.y = Mathf.Clamp(newPosition.y, -magicValue, magicValue);

            obj.localPosition = newPosition;
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