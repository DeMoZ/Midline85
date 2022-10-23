using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class MenuButtonView : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler,
        ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Color textHover = default;
        [SerializeField] private TextMeshProUGUI text = default;

        private Color _textColorNormal;
        private bool _isSelectedByEventSystem;

        public event Action OnClick;

        private void Start() =>
            _textColorNormal = text.color;

        public void OnPointerEnter(PointerEventData eventData) =>
            SetHoverColor(true);

        public void OnPointerExit(PointerEventData eventData) =>
            SetHoverColor(false);

        public void OnPointerClick(PointerEventData eventData)
        {
            SetHoverColor(false);
            OnClick?.Invoke();
        }

        public void OnSelect(BaseEventData eventData)
        {
            _isSelectedByEventSystem = true;
            SetHoverColor(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelectedByEventSystem = false;
            SetHoverColor(false);
        }

        private void SetHoverColor(bool hover)
        {
            text.color = hover ? textHover
                : _isSelectedByEventSystem ? textHover
                : _textColorNormal;
        }
    }
}