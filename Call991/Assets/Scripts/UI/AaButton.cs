using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public abstract class AaButton : Selectable, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        [SerializeField] private ButtonAudioSettings buttonAudioSettings = default;
        [SerializeField] private CursorSet cursorSettings = default;
        //[SerializeField] private Button button;

        public UnityEvent onButtonSelect = default;
        public UnityEvent onButtonClick = default;
        public UnityEvent onButtonNormal = default;
        
        public bool IsKeyboardSelected { get; private set; }
        public bool IsMouseSelected { get; private set; }
        public bool IsSelected { get; private set; }

        //public Button Button => button;

        protected abstract void OnButtonSelect();
        protected abstract void OnButtonClick();
        protected abstract void OnButtonNormal();
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"[{this}] Button {name} Hovered over the button");
            IsMouseSelected = true;
            if (!IsSelected)
            {
                OnButtonSelect();
                onButtonSelect?.Invoke();
            }
            
            IsSelected = true;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log($"[{this}] Button {name} Stopped hovering over the button");
            IsMouseSelected = false;
            IsSelected = IsKeyboardSelected;

            if (!IsSelected)
            {
                OnButtonNormal();
                onButtonNormal?.Invoke();
            }
        }
        
        public override void OnSelect(BaseEventData eventData)
        {
            Debug.Log($"[{this}] Button {name} selected");
            IsKeyboardSelected = true;

            if (!IsSelected)
            {
                OnButtonSelect();
                onButtonSelect?.Invoke();
            }
            
            IsSelected = true;
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            Debug.Log($"[{this}] Button {name} deselected");
            IsKeyboardSelected = false;
            IsSelected = IsMouseSelected;

            if (!IsSelected)
            {
                OnButtonNormal();
                onButtonNormal?.Invoke();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[{this}] Button {name} clicked");

            OnButtonClick();
            onButtonClick?.Invoke();
        }

        /// <summary>
        /// For keyboard and timeout press -> SetPressed
        /// </summary>
        public void Press()
        {
            buttonAudioSettings?.PlayClickSound();
            OnButtonClick();
            onButtonClick?.Invoke();
        }
        
        public void SetNormal()
        {
            //cursorSettings?.ApplyCursor(CursorType.Normal);
            OnButtonNormal();
            onButtonNormal?.Invoke();
        }
    }
}