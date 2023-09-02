using System;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace PhotoViewer.Scripts.Photo
{
    public class PhotoView : AaWindow
    {
        [SerializeField] private RectTransform viewTransfrom = default;
        [SerializeField] private Image image = default;
        [SerializeField] private RectTransform imageTransform = default;
        [SerializeField] private MenuButtonView closeBtn = default;
        [SerializeField] private NewspaperInput newspaperInput = default;
        [SerializeField] private NewspaperInputSo newspaperInputConfig = default;
       
        private Vector2 _initialImageSize;
        private bool _zoomIn;
        private Sequence _zoomSequence;

        public event Action OnClose;

        private Vector2 ViewerSize
        {
            get
            {
                var rect = viewTransfrom.rect;
                return new Vector2(rect.width, rect.height);
            }
        }

        private Vector2 ImageSize
        {
            get
            {
                var rect = imageTransform.rect;
                var angle = (int)imageTransform.rotation.eulerAngles.z;
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
            if (image)
                image.sprite = imageData.Sprite;

            _initialImageSize = ViewerSize;
        }

        private void ZoomOnClick(Vector2 clickPos)
        {
            _zoomIn = !_zoomIn;

            _zoomSequence?.Kill();
            _zoomSequence = DOTween.Sequence().SetEase(Ease.InOutCubic);
            _zoomSequence.SetUpdate(true);

            var zoomSize = _zoomIn ? _initialImageSize * newspaperInputConfig.MaxZoom : _initialImageSize;
            _zoomSequence.Append(imageTransform.DOSizeDelta(zoomSize, newspaperInputConfig.ZoomTime));

            var zoomPosition = _zoomIn ? CalculateZoomPosition(clickPos) : Vector2.zero;
            _zoomSequence.Insert(0, imageTransform.DOLocalMove(zoomPosition, newspaperInputConfig.ZoomTime));
        }

        // todo remove hardcode
        private Vector2 CalculateZoomPosition(Vector2 clickPos)
        {
            var relation = clickPos.y / Screen.height;
            var coordinates = 1500 - 3000 * relation;

            return new Vector2(0, coordinates);
        }

        // todo remove hardcode
        private void ApplyScroll(float zoomDelta)
        {
            if (!_zoomIn) return;

            Vector2 newPosition = imageTransform.localPosition;
            newPosition.y += zoomDelta;

            newPosition.x = Mathf.Clamp(newPosition.x, -1200, 1200);
            newPosition.y = Mathf.Clamp(newPosition.y, -1500, 1500);

            imageTransform.localPosition = newPosition;
        }


        private void ApplyMove(Vector2 deltaPosition)
        {
            if (!_zoomIn) return;

            Vector2 newPosition = imageTransform.localPosition;
            newPosition += deltaPosition;

            newPosition.x = Mathf.Clamp(newPosition.x, -1200, 1200);
            newPosition.y = Mathf.Clamp(newPosition.y, -1500, 1500);

            imageTransform.localPosition = newPosition;
        }
    }
}