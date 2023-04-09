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
            if (EventSystem.current == null) 
                return true;
            
            return EventSystem.current.currentSelectedGameObject != gameObject &&
                   EventSystem.current.gameObject != gameObject;
        }

        public static bool IsSelected(this GameObject gameObject)
        {
            if (EventSystem.current == null) 
                return false;

            return EventSystem.current.currentSelectedGameObject == gameObject;
        }

        public static string ToStringEventSystem(this GameObject go)
        {
            return $" <color=green>{go.name}</color>; current {EventSystemExtension.ToString()}";
        }
    }

    public static class EventSystemExtension
    {
        public new static string ToString()
        {
            if (EventSystem.current == null) 
                return $"<color=green> no current</color>";
            
            return ($"<color=green> first [{EventSystem.current.firstSelectedGameObject?.name??""}];" +
                    $" current [{EventSystem.current.currentSelectedGameObject?.name??""}];" +
                    $" navigation {EventSystem.current.sendNavigationEvents} </color>");
        }

        // public static bool IsPointerOverGameObject(this GameObject obj)
        // {
        //     return false;
        // }
    }
}