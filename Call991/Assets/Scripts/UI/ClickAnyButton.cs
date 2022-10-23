using System;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class ClickAnyButton : MonoBehaviour
    {
        public UnityEvent onAnyButton;
        public event Action OnClick;

        public void Update()
        {
            if (Input.anyKey)
            {
                OnClick?.Invoke();
                onAnyButton?.Invoke();
            }
        }
    }
}