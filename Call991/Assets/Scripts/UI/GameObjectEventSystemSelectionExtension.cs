using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public static class GameObjectEventSystemSelectionExtension
    {
        public static void Select(this GameObject gameObject)
        {
            EventSystem.current.firstSelectedGameObject = gameObject;
            EventSystem.current.SetSelectedGameObject(gameObject);
            EventSystem.current.sendNavigationEvents = true;
        }

        public static void Pressed(this GameObject gameObject)
        {
            EventSystem.current.firstSelectedGameObject = gameObject;
            EventSystem.current.SetSelectedGameObject(gameObject);
            EventSystem.current.sendNavigationEvents = true;
        }

        public static void StopSelection()
        {
            EventSystem.current.firstSelectedGameObject = null;
            EventSystem.current.sendNavigationEvents = false;
        }

        public static void ClearSelection()
        {
            EventSystem.current.firstSelectedGameObject = null;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.sendNavigationEvents = false;
        }

        public static bool NotSelected(this GameObject gameObject)
        {
            return EventSystem.current.firstSelectedGameObject != gameObject &&
                   EventSystem.current.currentSelectedGameObject != gameObject &&
                   EventSystem.current.gameObject != gameObject;
        }
    }
}