using System;
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
        private Vector2 _zoomLimit = new(0.8f, 5);

        private Vector2 _initialImageSize;
        private Vector2 _imageSize;

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
                var angle = (int) _imageTransform.rotation.eulerAngles.z;
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
            _newspaperInput.onZoom += ApplyZoom;
            _closeBtn.OnClick += Close;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _newspaperInput.onDrag -= ApplyMove;
            _newspaperInput.onZoom -= ApplyZoom;
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

            RescalePhoto(imageData.Sprite);
        }
        
        private void ApplyZoom(float value)
        {
            value *= _initialImageSize.y;
            _imageSize.y += value;
            var magnitudeRelation = _imageSize.y / _initialImageSize.y;
            magnitudeRelation = Mathf.Clamp(magnitudeRelation, _zoomLimit.x, _zoomLimit.y);

            var y = _initialImageSize.y * magnitudeRelation;
            var x = (_initialImageSize.x / _initialImageSize.y) * y;

            _imageSize = new Vector2(x, y);
            _imageTransform.sizeDelta = _imageSize;
        }

        private void ApplyMove(Vector2 deltaPosition)
        {
            Vector2 newPosition = _imageTransform.localPosition;

            if (ImageSize.x > ViewerSize.x)
            {
                newPosition.x += deltaPosition.x;

                if ((newPosition.x - ImageSize.x / 2) > -ViewerSize.x / 2)
                    newPosition.x = -ViewerSize.x / 2 + ImageSize.x / 2;

                if ((newPosition.x + ImageSize.x / 2) < ViewerSize.x / 2)
                    newPosition.x = ViewerSize.x / 2 - ImageSize.x / 2;
            }
            else
                newPosition.x = 0;

            if (ImageSize.y > ViewerSize.y)
            {
                newPosition.y += deltaPosition.y;

                if ((newPosition.y - ImageSize.y / 2) > -ViewerSize.y / 2)
                    newPosition.y = -ViewerSize.y / 2 + ImageSize.y / 2;

                if ((newPosition.y + ImageSize.y / 2) < ViewerSize.y / 2)
                    newPosition.y = ViewerSize.y / 2 - ImageSize.y / 2;
            }
            else
                newPosition.y = 0;

            _imageTransform.localPosition = (Vector3) newPosition;
        }

        private void RescalePhoto(Sprite sprite)
        {
            return;
            if (_viewTransfrom == null) return;

            var viewerSize = ViewerSize;
            var spriteSize = new Vector2(sprite.rect.width, sprite.rect.height);

            var viewerAspect = viewerSize.x / viewerSize.y;
            var spriteAspect = spriteSize.x / spriteSize.y;

            if (spriteAspect > viewerAspect)
            {
                var relation = viewerSize.x / sprite.texture.width;
                _imageTransform.sizeDelta = new Vector2(viewerSize.x, relation * spriteSize.y);
            }
            else
            {
                var relate = viewerSize.y / sprite.texture.height;
                _imageTransform.sizeDelta = new Vector2(relate * spriteSize.x, viewerSize.y);
            }

            _initialImageSize = _imageTransform.sizeDelta;
            _imageSize = _initialImageSize;
        }
    }
}