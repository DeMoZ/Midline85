using UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class AaWindow : InputHandler
{
    [SerializeField] private AaSelectable[] windowSelectables = default;

    protected virtual void OnEnable()
    {
        // reset any selected object in case
        EventSystem.current.firstSelectedGameObject = null;
        EventSystem.current.SetSelectedGameObject(null);
        
        foreach (var selectable in windowSelectables)
        {
            selectable.OnSelect += OnSelect;
            selectable.OnUnSelect += OnUnSelect;
        }
    }

    protected virtual void OnDisable()
    {
        foreach (var selectable in windowSelectables)
        {
            selectable.OnSelect -= OnSelect;
            selectable.OnUnSelect -= OnUnSelect;
        }
    }
    
    private void OnUnSelect(AaSelectable obj)
    {
        Debug.Log($"<color=red>Window</color> to OnUnSelect {obj.gameObject.ToStringEventSystem()}");
        firstSelected = obj;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnSelect(AaSelectable obj)
    {
        Debug.Log($"<color=red>Window</color> to OnSelect {obj.gameObject.ToStringEventSystem()}");

        firstSelected = obj;
        
        if(!EventSystem.current.alreadySelecting)
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    }
}
