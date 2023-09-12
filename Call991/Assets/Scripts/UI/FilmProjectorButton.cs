using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class FilmProjectorButton : Button
    {
        [SerializeField] private CursorSet cursorSet = default;
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            cursorSet.ApplyCursor(CursorType.CanClick);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            cursorSet.ApplyCursor(CursorType.Normal);
        }
    }
}