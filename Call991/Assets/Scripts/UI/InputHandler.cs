using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public abstract class InputHandler : MonoBehaviour
    {
        [SerializeField] protected AaButton firstSelected = default;

        private static readonly KeyCode[] _leftKeyCodes = { KeyCode.LeftArrow, KeyCode.A };
        private static readonly KeyCode[] _rightKeyCodes = { KeyCode.RightArrow, KeyCode.D };

        private static readonly KeyCode[] _pressKeyCodes = { KeyCode.Space, KeyCode.Return };
        private static readonly KeyCode[] _ignoreKeyCodes = { KeyCode.Escape };
        private static readonly string[] _mouseAxis = { "Mouse X", "Mouse Y" };
        private static readonly int[] _mouseButtons = { 0, 1, 2 };

        private bool? _mouseEnabled = true;
        private bool _mouseVisible = true;

        private bool MouseControlled
        {
            set
            {
                if (_mouseEnabled == value) return;

                Debug.Log("[InputHandler] mouse handler");

                _mouseVisible = value;
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
            _mouseEnabled = null;
            Cursor.visible = _mouseVisible;
        }

        protected virtual void Update()
        {
            if (IsMouseMoved(_mouseAxis) || GetMouseButtonDown(_mouseButtons))
                MouseControlled = true;

            if (IsKeyboardControlled())
                MouseControlled = false;

            if (GetPressedButtons(_pressKeyCodes))
                PressObject();

            if (firstSelected && EventSystem.current &&
                firstSelected.gameObject == EventSystem.current.currentSelectedGameObject)
            //if (firstSelected.gameObject == EventSystem.current.currentSelectedGameObject)
            {
                if (GetPressedButtons(_leftKeyCodes))
                    OnLeftButtonPress(firstSelected);

                if (GetPressedButtons(_rightKeyCodes))
                    OnRightButtonPress(firstSelected);
            }
        }

        public static bool GetPressLeft(GameObject gobject) =>
            gobject.gameObject == EventSystem.current?.currentSelectedGameObject && GetPressedButtons(_leftKeyCodes);

        public static bool GetPressRight(GameObject gobject) =>
            gobject.gameObject == EventSystem.current?.currentSelectedGameObject && GetPressedButtons(_rightKeyCodes);

        private bool IsKeyboardControlled() =>
            Input.anyKeyDown &&
            !GetMouseButtonDown(_mouseButtons) &&
            !GetPressedButtons(_pressKeyCodes) && !GetPressedButtons(_ignoreKeyCodes);

        private static bool GetPressedButtons(IEnumerable<KeyCode> keyCodes) =>
            keyCodes.Any(Input.GetKeyDown);

        private static bool GetMouseButtonDown(IEnumerable<int> mouseButtons) =>
            mouseButtons.Any(Input.GetMouseButtonDown);

        private static bool IsMouseMoved(IEnumerable<string> mouseAxis) =>
            mouseAxis.Any(axis => Input.GetAxis(axis) != 0);

        private void SelectFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }

        private void PressObject()
        {
            // Debug.LogWarning($"[InputHandler] 0 PressObject; firstSelected = {firstSelected}");
            if (gameObject.activeInHierarchy && firstSelected.gameObject.IsSelected())
            {
                Debug.Log($"[InputHandler] 1 PressObject.Press; firstSelected = {firstSelected}");
                firstSelected.Press();
            }
        }

        protected virtual void OnLeftButtonPress(AaButton aaSelectable)
        {
        }

        protected virtual void OnRightButtonPress(AaButton aaSelectable)
        {
        }
    }
}