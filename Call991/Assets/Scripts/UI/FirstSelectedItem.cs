using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class FirstSelectedItem : MonoBehaviour
    {
        public GameObject objectToFirstSelect;

        private void OnEnable()
        {
            EventSystem.current.firstSelectedGameObject = null;
        }

        public void SelectFirst()
        {
            EventSystem.current.firstSelectedGameObject = objectToFirstSelect;
            EventSystem.current.SetSelectedGameObject(objectToFirstSelect);
            EventSystem.current.sendNavigationEvents = true;
        }
    }
}