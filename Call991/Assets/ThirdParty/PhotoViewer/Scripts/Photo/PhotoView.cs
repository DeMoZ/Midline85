using System;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace PhotoViewer.Scripts.Photo
{
    public class PhotoView : AaWindow
    {
        [SerializeField] private RectTransform _viewTransfrom = default;
        [SerializeField] private Image _image = default;
        [SerializeField] private RectTransform _imageTransform = default;
        [SerializeField] private MenuButtonView _closeBtn = default;
        [SerializeField] private NewspaperInput _newspaperInput = default;

        [SerializeField] private float _zoomTime = 0.5f;

        // [SerializeField]
        private Vector2 _zoomLimit = new(1, 2.3f);

        private Vector2 _initialImageSize;
        private bool _zoomIn;
        private Sequence _zoomSequence;

        public event Action OnClose;

        private Vector2 ViewerSize
        {
            get
            {
                var rect = _viewTransfrom.rect;
                return new Vector2(rect.width, rect.height);
            }
        }

        private Vector2 ImageSize
        {
            get
            {
                var rect = _imageTransform.rect;
                var angle = (int)_imageTransform.rotation.eulerAngles.z;
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

            _newspaperInput.onDrag += ApplyMove;
            //_newspaperInput.onZoom += ApplyZoom;
            _newspaperInput.onClick += ZoomOnClick;
            _closeBtn.OnClick += Close;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _newspaperInput.onDrag -= ApplyMove;
            //_newspaperInput.onZoom -= ApplyZoom;
            _newspaperInput.onClick -= ZoomOnClick;
            _closeBtn.OnClick -= Close;
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
            if (_image)
                _image.sprite = imageData.Sprite;

            _initialImageSize = ViewerSize;
        }

        private void ZoomOnClick(Vector2 clickPos)
        {
            _zoomIn = !_zoomIn;

            _zoomSequence?.Kill();
            _zoomSequence = DOTween.Sequence().SetEase(Ease.InOutCubic);
            _zoomSequence.SetUpdate(true);

            var zoomSize = _zoomIn ? _initialImageSize * _zoomLimit.y : _initialImageSize;
            _zoomSequence.Append(_imageTransform.DOSizeDelta(zoomSize, _zoomTime));

            var zoomPosition = _zoomIn ? CalculateZoomPosition(clickPos) : Vector2.zero;
            _zoomSequence.Insert(0, _imageTransform.DOLocalMove(zoomPosition, _zoomTime));
        }

        private Vector2 CalculateZoomPosition(Vector2 clickPos)
        {
            var relation = clickPos.y / Screen.height;
            var coordinates = 1500 - 3000 * relation;

            return new Vector2(0, coordinates);
        }

        private void ApplyMove(Vector2 deltaPosition)
        {
            if (!_zoomIn) return;
            
            Vector2 newPosition = _imageTransform.localPosition;
            newPosition += deltaPosition;

            newPosition.x = Mathf.Clamp(newPosition.x, -1200, 1200);
            newPosition.y = Mathf.Clamp(newPosition.y, -1500, 1500);

            _imageTransform.localPosition = newPosition;
        }
    }
}