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

        public static void NoSelection()
        {
            EventSystem.current.firstSelectedGameObject = null;
            //EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.sendNavigationEvents = false;
        }
    }
}