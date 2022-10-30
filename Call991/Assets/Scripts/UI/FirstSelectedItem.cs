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
            objectToFirstSelect.Select();
        }
    }
}