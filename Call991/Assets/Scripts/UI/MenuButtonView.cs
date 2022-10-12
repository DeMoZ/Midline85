using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class MenuButtonView : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
    {
        [SerializeField] private Color textHover = default;
        [SerializeField] private TextMeshProUGUI text = default;

        private Color _textColorNormal;
        public event Action OnClick;

        private void Start()
        {
            _textColorNormal = text.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            text.color = textHover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            text.color = _textColorNormal;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            text.color = _textColorNormal;
            OnClick?.Invoke();
        }
    }
}