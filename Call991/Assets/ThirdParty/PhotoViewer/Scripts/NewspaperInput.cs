using System;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PhotoViewer.Scripts
{
    public class NewspaperInput : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
#if UNITY_IOS || UNITY_ANDROID
        private float _moveSpeed = 5;
#else
        private float _moveSpeed = 25;
#endif
        [SerializeField] private CursorSet _cursorSettings = default;
        [SerializeField] private float _moveDumping = 3f;
        [Space]
        [SerializeField] private float _zoomSpeed = 50f;

        private float _moveDelta;
        private float _zoomDelta;
        
        public Action<Vector2> onDrag;
        //public Action<float> onZoom;
        public Action<Vector2> onClick;

        private Vector2 _deltaPosition = Vector2.zero;
        private bool _hover;

        
        private void Update()
        {
            _deltaPosition = Vector2.Lerp(_deltaPosition, Vector2.zero, Time.deltaTime * _moveDumping);

            if (_deltaPosition.magnitude < 0.1f)
                _deltaPosition = Vector2.zero;

            onDrag?.Invoke(_deltaPosition);
            
            //var zoomDelta = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * _zoomSpeed;
            //onZoom?.Invoke(zoomDelta);
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

        public void OnBeginDrag(PointerEventData eventData) => 
            _cursorSettings.ApplyCursor(CursorType.Drag);

        public void OnDrag(PointerEventData eventData) => 
            _deltaPosition = eventData.delta * Time.deltaTime * _moveSpeed;

        public void OnEndDrag(PointerEventData eventData) => 
            EndInteraction();

        private Vector2? _startClickPosition;
        public void OnPointerDown(PointerEventData eventData)
        {
            _cursorSettings.ApplyCursor(CursorType.Drag);
            _startClickPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            EndInteraction();
            if (_startClickPosition.HasValue && Vector2.Distance(_startClickPosition.Value, eventData.position) < 0.5f)
                onClick?.Invoke(eventData.position);

            _startClickPosition = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //onClick?.Invoke(eventData.position);
        }
        
        private void EndInteraction()
        {
            if (_hover)
                _cursorSettings.ApplyCursor(CursorType.CanDrag);
            else
                _cursorSettings.ApplyCursor(CursorType.Normal);
        }
    }
}