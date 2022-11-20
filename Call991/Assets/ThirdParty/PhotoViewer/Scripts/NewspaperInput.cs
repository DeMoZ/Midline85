using System;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PhotoViewer.Scripts
{
    public class NewspaperInput : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IEndDragHandler
    {
#if UNITY_IOS || UNITY_ANDROID
        private float _moveSpeed = 5;
#else
        private float _moveSpeed = 25;
#endif
        [SerializeField] private CursorSet _cursorSettings = default;
        [SerializeField] private float _moveDumping = 3f;
        [Space]
        [SerializeField] private float _zoomSpeed = 25f;
        [SerializeField] private float _zoomDumping = 4f;

        private float _moveDelta;
        private float _zoomDelta;
        
        public Action<Vector2> onDrag;
        public Action<float> onZoom;

        private Vector2 _deltaPosition = Vector2.zero;
        private bool _hover;

        
        private void Update()
        {
            _deltaPosition = Vector2.Lerp(_deltaPosition, Vector2.zero, Time.deltaTime * _moveDumping);

            if (_deltaPosition.magnitude < 0.1f)
                _deltaPosition = Vector2.zero;

            onDrag?.Invoke(_deltaPosition);
            
            var zoomDelta = -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * _zoomSpeed;
            zoomDelta = Mathf.Lerp(_zoomDelta, zoomDelta, Time.deltaTime * _zoomDumping);
            onZoom?.Invoke(zoomDelta);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
            _cursorSettings.ApplyCursor(CursorType.CanDrag);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
            _cursorSettings.ApplyCursor(CursorType.Normal);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _cursorSettings.ApplyCursor(CursorType.Drag);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _deltaPosition = eventData.delta * Time.deltaTime * _moveSpeed;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_hover)
                _cursorSettings.ApplyCursor(CursorType.CanDrag);
        }
    }
}