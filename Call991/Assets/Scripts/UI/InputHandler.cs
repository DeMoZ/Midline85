using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public abstract class InputHandler : MonoBehaviour
    {
        [SerializeField] public AaSelectable firstSelected = default;

        private bool mouseEnabled = true;
        
        private bool MouseControlled
        {
            set
            {
                if (mouseEnabled != value)
                {
                    Debug.Log("mouse handler");

                    // if (!value)
                    // {
                    //     _savedMousePosition.x = (int)Input.mousePosition.x;
                    //     _savedMousePosition.y = (int)Input.mousePosition.y;
                    // }

                    mouseEnabled = value;
                    Cursor.visible = value;
                    Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;

                    if (!value)
                    {
                        SelectFirstSelected();
                    }

                    // if (value)
                    // {
                    //     SetCursorPos(_savedMousePosition.x, _savedMousePosition.y);
                    // }
                }
            }
        }

        protected virtual void Update()
        {
            if (IsMouseMoved() || GetMouseButtonDown())
                MouseControlled = true;

            if (IsKeyboardControlled())
                MouseControlled = false;

            if (GetPressButtons())
                PressObject();
        }

        private static bool IsKeyboardControlled()
        {
            return Input.anyKeyDown && !GetMouseButtonDown() && !GetPressButtons();
        }

        private static bool GetPressButtons()
        {
            return Input.GetKeyDown(KeyCode.Space) ||
                   Input.GetKeyDown(KeyCode.Return);
        }

        private static bool GetMouseButtonDown()
        {
            return Input.GetMouseButtonDown(0) ||
                   Input.GetMouseButtonDown(1) ||
                   Input.GetMouseButtonDown(2);
        }

        private static bool IsMouseMoved()
        {
            return Input.GetAxis("Mouse X") != 0 ||
                   Input.GetAxis("Mouse Y") != 0;
        }

        private void SelectFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }

        private void PressObject()
        {
            if (gameObject.activeInHierarchy)
            {
                if (firstSelected.gameObject.IsSelected())
                {
                   // MouseControlled = true;
                    
                    firstSelected.Press();
                    
                   // MouseControlled = false;
                }
            }
        }
    }
}