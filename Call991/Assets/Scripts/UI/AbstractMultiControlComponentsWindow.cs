using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class AbstractMultiControlComponentsWindow : MonoBehaviour
    {
        [SerializeField] protected GameObject firstSelected = default;
        [SerializeField] private AbstractMySelectable[] windowSelectables = default;

        protected virtual void OnEnable()
        {
            //StartCoroutine(YieldForClick());

            foreach (var selectable in windowSelectables)
            {
                if(selectable!=null)
                    selectable.OnAnySelect += OnAnySelect;
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var selectable in windowSelectables)
            { 
                if(selectable!=null)
                    selectable.OnAnySelect -= OnAnySelect;
            }
        }

        private void OnAnySelect(GameObject gameObj)
        {
            firstSelected = gameObj;
        }

        private IEnumerator YieldForClick()
        {
            while (!Input.anyKeyDown)
                yield return null;

            EventSystem.current.SetSelectedGameObject(firstSelected);
            EventSystem.current.firstSelectedGameObject = firstSelected;
            firstSelected.Select();
        }
    }
}