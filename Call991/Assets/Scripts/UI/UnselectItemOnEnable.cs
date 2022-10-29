using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UnselectItemOnEnable : MonoBehaviour
    {
        private void OnEnable()
        {
            EventSystem.current.firstSelectedGameObject = null;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.sendNavigationEvents = true;
        }
    }
}