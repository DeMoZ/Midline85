using System.Collections.Generic;
using PhotoViewer.Scripts.Photo;
using UnityEngine;

namespace PhotoViewer.Scripts
{
    public class PhotoViewer : MonoBehaviour
    {
        [SerializeField] private PhotoView _photoView = default;

        [SerializeField] private GameObject _btnReturn = default;
        [SerializeField] private GameObject _btnPrev = default;
        [SerializeField] private GameObject _btnNext = default;

        private List<ImageData> _images = new List<ImageData>();
        private int CurrentPhoto { get; set; }

        private ImageData _currentImageData;

        private void Start() =>
            _btnReturn.SetActive(false);

        public void CloseViewer()
        {
            Clear();
            gameObject.SetActive(false);
        }

        public void AddImageData(ImageData data) =>
            _images.Add(data);

        public void AddImageData(List<ImageData> data) =>
            _images.AddRange(data);

        public void Clear()
        {
            _images.Clear();

            _currentImageData = new ImageData();
            _photoView.Clear();
        }

        public void Show()
        {
            ShowImage(_images[0]);
        }

        private void SetActiveButtons(bool active)
        {
            _btnReturn.SetActive(active);
            _btnPrev.SetActive(active);
            _btnNext.SetActive(active);
        }

        private void ShowImage(ImageData imageData)
        {
            _currentImageData = imageData;
            _photoView.gameObject.SetActive(true);
            _photoView.Show(imageData);
        }
    }
}