using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class NewspaperCursor : MonoBehaviour, IPointerEnterHandler, IDragHandler, IPointerExitHandler, IBeginDragHandler,
        IEndDragHandler
    {
        [SerializeField] private CursorSet cursorSettings = default;

        private bool _hover;

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

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            cursorSettings.ApplyCursor(CursorType.Drag);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (_hover)
                cursorSettings.ApplyCursor(CursorType.CanDrag);
        }
    }
}