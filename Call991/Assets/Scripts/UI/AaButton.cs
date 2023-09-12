using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public abstract class AaButton : Selectable, IPointerClickHandler
    {
        [SerializeField] private ButtonAudioSettings buttonAudioSettings = default;
        [SerializeField] private CursorSet cursorSettings = default;

        private float _clickDelay = 0.5f;
        private float _lastClick;
        protected abstract void OnButtonSelect();
        protected abstract void OnButtonClick();
        protected abstract void OnButtonNormal();
        
        public UnityEvent onButtonSelect = default;
        public UnityEvent onButtonClick = default;
        public UnityEvent onButtonNormal = default;
        
        public bool IsKeyboardSelected { get; private set; }
        public bool IsMouseSelected { get; private set; }
        public bool IsSelected { get; private set; }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"[{this}] Button {name} Hovered over the button");
            IsMouseSelected = true;
            if (!IsSelected)
            {
                OnButtonSelect();
                onButtonSelect?.Invoke();
            }
            
            cursorSettings.ApplyCursor(CursorType.CanClick);
            IsSelected = true;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log($"[{this}] Button {name} Stopped hovering over the button");
            IsMouseSelected = false;
            IsSelected = IsKeyboardSelected;

            //if (!InteractionDelay()) return;
            
            if (!IsSelected)
            {
                OnButtonNormal();
                onButtonNormal?.Invoke();
            }
            
            cursorSettings.ApplyCursor(CursorType.Normal);
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
            if (!InteractionDelay()) return;
            
            Press();
        }

        /// <summary>
        /// For keyboard and timeout press -> SetPressed
        /// </summary>
        public void Press()
        {
            if (!interactable) return;
            
            Debug.Log($"[{this}] <color=yellow>Press</color> Button {name} clicked");
            _lastClick = Time.time;
            
            buttonAudioSettings.PlayClickSound();
            OnButtonClick();
            onButtonClick?.Invoke();
        }
        
        public void SetNormal()
        {
            OnButtonNormal();
            onButtonNormal?.Invoke();
        }

        public void Reset()
        {
            IsSelected = false;
            IsMouseSelected = false;
            IsKeyboardSelected = false;
            SetNormal();
        }

        private bool InteractionDelay()
        {
            return _lastClick + _clickDelay < Time.time;
        }
    }
}