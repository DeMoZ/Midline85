using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI
{
    public class ClickAnyButton : MonoBehaviour
    {
        public bool onlyFirstSelection;
        public UnityEvent onAnyButton;
        public event Action OnClick;

        public void Update()
        {
            if (Input.anyKeyDown)
            {
                if (Input.GetMouseButton(0) || Input.GetMouseButton(0))
                    return;
                
                if (onlyFirstSelection && EventSystem.current.firstSelectedGameObject)
                    return;
                
                OnClick?.Invoke();
                onAnyButton?.Invoke();
            }
        }
    }
}