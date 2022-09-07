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
    
        private Color _testNormal;
        public event Action OnClick;

        private void Start()
        {
            _testNormal = text.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            text.color = textHover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            text.color = _testNormal;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }
    }
}