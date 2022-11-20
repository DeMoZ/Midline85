using System;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace PhotoViewer.Scripts.Photo
{
    public class PhotoView : MonoBehaviour
    {
        [SerializeField] private RectTransform _viewTransfrom = default;
        [SerializeField] private Image _image = default;
        [SerializeField] private RectTransform _imageTransform = default;
        [SerializeField] private MenuButtonView _closeBtn = default;
        [SerializeField] private NewspaperInput _newspaperInput = default;

        private Vector2? _defaultImageSize;

        public Action OnChange;
        private ImageData _currentData;

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

        private void OnEnable()
        {
            _newspaperInput.onDrag += ApplyMove;
            _newspaperInput.onZoom += ApplyZoom;
            _closeBtn.OnClick += Close;
        }

        private void OnDisable()
        {
            _newspaperInput.onDrag -= ApplyMove;
            _newspaperInput.onZoom -= ApplyZoom;
            _closeBtn.OnClick -= Close;
        }

        private void Close()
        {
            OnClose?.Invoke();
        }

        public void SetNewspaper(Sprite sprite)
        {
            var imageData = new ImageData
            {
                Sprite = sprite,
            };

            ShowData(imageData);
        }

        public void Show(ImageData imageData)
        {
            _currentData = imageData;
            ShowData(imageData);
        }

        public void Reset() =>
            Show(_currentData);

        private void ShowData(ImageData imageData)
        {
            if (_image)
                _image.sprite = imageData.Sprite;

            RescalePhoto(imageData.Sprite);
        }

        private void ApplyZoom(float value)
        {
            _imageTransform.sizeDelta = (Vector2) (_defaultImageSize + _defaultImageSize * value * 10);
            _defaultImageSize = _imageTransform.sizeDelta;
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

            if (deltaPosition != Vector2.zero)
                OnChange?.Invoke();
        }

        private void RescalePhoto(Sprite sprite)
        {
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

            _defaultImageSize = _imageTransform.sizeDelta;
        }
    }
}