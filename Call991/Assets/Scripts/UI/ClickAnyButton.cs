using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI
{
    public class ClickAnyButton : MonoBehaviour
    {
        [SerializeField] private bool theKey;
        [SerializeField] [ShowIf("theKey")] private KeyCode keyCode;

        public bool onlyFirstSelection;
        public UnityEvent onAnyButton;
        public event Action OnClick;

        public void Update()
        {
            if (theKey)
            {
                if (!Input.GetKeyDown(keyCode))
                    return;
                
                if (onlyFirstSelection && EventSystem.current.firstSelectedGameObject)
                    return;

                OnClick?.Invoke();
                onAnyButton?.Invoke();
            }
            else if (Input.anyKeyDown)
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