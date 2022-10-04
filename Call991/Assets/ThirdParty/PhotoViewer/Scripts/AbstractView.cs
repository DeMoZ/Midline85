using System;
using UnityEngine;
using UnityEngine.UI;

namespace PhotoViewer.Scripts
{
    public abstract class AbstractView : MonoBehaviour
    {
        [SerializeField] protected Slider _zoomSlider = default;
 
        public event Action OnShow;

        protected abstract void Zoom(float value);
        public abstract void ApplyInput(Vector2 deltaPosition);

        protected Action OnChange;

        protected abstract void ShowData(ImageData imageData);

        protected ImageData _currentData;

        public Slider ZoomSlider  { 
            get=>_zoomSlider;
            set=>_zoomSlider=value; 
        }

        public void Show(ImageData imageData)
        {
            _currentData = imageData;
            ShowData(imageData);
            OnShow?.Invoke();
        }

        public void Reset() => 
            Show(_currentData);

        protected void ResetZoom(float value = 0)
        {
            _zoomSlider.onValueChanged.RemoveListener(Zoom);
            _zoomSlider.value = value;
            _zoomSlider.onValueChanged.AddListener(Zoom);
        }

        protected void OnDestroy() =>
            _zoomSlider.onValueChanged.RemoveAllListeners();
    }
}