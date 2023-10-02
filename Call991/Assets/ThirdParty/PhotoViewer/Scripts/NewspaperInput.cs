using System;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PhotoViewer.Scripts
{
    public class NewspaperInput : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private CursorSet cursorSettings = default;

        private Vector2 _targetScroll;
        private Vector2 _currentScroll;
        private Vector2 _scrollVelocity = Vector2.zero;

        private Vector2? _startClickPosition;
        private NewspaperInputSo _config;
        private float _moveDelta;
        private float _zoomDelta;

        //public Action<Vector2> onDrag;
        public Action<float> onScroll;
        public Action<Vector2> onClick;

        private Vector2 _deltaPosition = Vector2.zero;
        private bool _hover;

        public void Init(NewspaperInputSo config)
        {
            _config = config;
        }

        private void Update()
        {
            var scrollInput = Input.GetAxis("Mouse ScrollWheel");
            _currentScroll.y -= scrollInput * _config.ScrollSpeed * Time.deltaTime;
            _currentScroll = Vector2.SmoothDamp(_currentScroll, Vector2.zero, ref _scrollVelocity,
                _config.ScrollSmoothTime);
            onScroll?.Invoke(_currentScroll.y);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
            cursorSettings.ApplyCursor(CursorType.CanDrag);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
            cursorSettings.ApplyCursor(CursorType.Normal);
        }

        public void OnBeginDrag(PointerEventData eventData) =>
            cursorSettings.ApplyCursor(CursorType.Drag);

        public void OnDrag(PointerEventData eventData) =>
            _deltaPosition = eventData.delta * Time.deltaTime * _config.MoveSpeed;

        public void OnEndDrag(PointerEventData eventData) =>
            EndInteraction();

        public void OnPointerDown(PointerEventData eventData)
        {
            cursorSettings.ApplyCursor(CursorType.Drag);
            _startClickPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            EndInteraction();
            // if (_startClickPosition.HasValue && Vector2.Distance(_startClickPosition.Value, eventData.position) < 10f)
            //     onClick?.Invoke(eventData.position);

            _startClickPosition = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(eventData.position);
        }

        private void EndInteraction() => cursorSettings.ApplyCursor(_hover ? CursorType.CanDrag : CursorType.Normal);
    }
}