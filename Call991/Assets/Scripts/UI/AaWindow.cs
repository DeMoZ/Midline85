using System.Threading;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class AaWindow : InputHandler
{
    [SerializeField] private AaSelectable[] windowSelectables = default;

    protected CancellationTokenSource tokenSource;

    private void Awake()
    {
        tokenSource = new CancellationTokenSource();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // reset any selected object in case
        EventSystem.current.firstSelectedGameObject = null;
        EventSystem.current.SetSelectedGameObject(null);

        foreach (var selectable in windowSelectables)
        {
            selectable.OnSelectObj += OnSelectObj;
            selectable.OnUnSelect += OnUnSelect;
        }
    }

    protected virtual void OnDisable()
    {
        foreach (var selectable in windowSelectables)
        {
            selectable.OnSelectObj -= OnSelectObj;
            selectable.OnUnSelect -= OnUnSelect;
        }
    }

    private void OnUnSelect(AaSelectable obj)
    {
        //Debug.Log($"[{this}] <color=red>Window</color> to OnUnSelect {obj.gameObject.ToStringEventSystem()}");
        firstSelected = obj;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnSelectObj(AaSelectable obj)
    {
        //Debug.Log($"[{this}] <color=red>Window</color> to OnSelect {obj.gameObject.ToStringEventSystem()}");
        firstSelected = obj;

        if (!EventSystem.current.alreadySelecting)
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    }

    private void OnDestroy()
    {
        tokenSource.Cancel();
    }
}