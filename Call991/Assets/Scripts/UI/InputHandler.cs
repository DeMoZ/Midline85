using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public abstract class InputHandler : MonoBehaviour
    {
        [SerializeField] protected AaSelectable firstSelected = default;

        private readonly KeyCode[] _pressKeyCodes = {KeyCode.Space, KeyCode.Return};
        private readonly KeyCode[] _ignoreKeyCodes = {KeyCode.Escape};
        private readonly string[] _mouseAxis = {"Mouse X", "Mouse Y"};
        private readonly int[] _mouseButtons = {0, 1, 2};

        private bool _mouseEnabled = true;

        private bool MouseControlled
        {
            set
            {
                if (_mouseEnabled == value) return;

                Debug.Log("[InputHandler] mouse handler");

                _mouseEnabled = value;
                Cursor.visible = value;
                Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;

                if (!value)
                {
                    SelectFirstSelected();
                }
            }
        }

        protected virtual void OnEnable()
        {
            // if (!_mouseEnabled)
            //     SelectFirstSelected();
        }
        
        protected virtual void Update()
        {
            if (IsMouseMoved(_mouseAxis) || GetMouseButtonDown(_mouseButtons))
                MouseControlled = true;

            if (IsKeyboardControlled())
                MouseControlled = false;

            if (GetPressedButtons(_pressKeyCodes))
                PressObject();
        }

        private bool IsKeyboardControlled()
        {
            return Input.anyKeyDown && !GetMouseButtonDown(_mouseButtons) &&
                   !GetPressedButtons(_pressKeyCodes) && !GetPressedButtons(_ignoreKeyCodes);
        }

        private static bool GetPressedButtons(IEnumerable<KeyCode> keyCodes)
        {
            return keyCodes.Any(Input.GetKeyDown);
        }

        private static bool GetMouseButtonDown(IEnumerable<int> mouseButtons)
        {
            return mouseButtons.Any(Input.GetMouseButtonDown);
        }

        private static bool IsMouseMoved(IEnumerable<string> mouseAxis)
        {
            return mouseAxis.Any(axis => Input.GetAxis(axis) != 0);
        }

        private void SelectFirstSelected()
        {
            Debug.Log($"[InputHandler] SelectFirstSelected, firstSelected = {firstSelected}");
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }

        private void PressObject()
        {
            if (gameObject.activeInHierarchy && firstSelected.gameObject.IsSelected())
            {
                firstSelected.Press();
            }
        }
    }
}