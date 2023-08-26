using System.Threading;
using DG.Tweening;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class AaWindow : InputHandler
{
    [Space]
    [SerializeField] private bool appearWithTween;
    [ShowIf("IsAppearAnimationWithUnityAnimation")]
    [SerializeField] private Animation appearAnimation = default;
    [Space]
    [ShowIf("appearWithTween")]
    [SerializeField] private RectTransform buttonsGroup = default;
    [ShowIf("appearWithTween")]
    [SerializeField] private CanvasGroup buttonsCanvas = default;
    [ShowIf("appearWithTween")]
    [SerializeField] private float animationTime = 0.5f;
    [ShowIf("appearWithTween")]
    [SerializeField] private float toPositionX = 218f;
    
    [Space] [Space]
    [SerializeField] private AaSelectable[] windowSelectables = default;

    private Sequence _appearSequence;
    
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

        if (appearWithTween)
        {
            _appearSequence?.Kill();
            _appearSequence = DOTween.Sequence();
            
            _appearSequence.SetUpdate(true);
            var position = buttonsGroup.position;
            position.x = 0;
            buttonsGroup.position = position; 
            buttonsCanvas.alpha = 0;
            _appearSequence.Append(buttonsGroup.DOMoveX(toPositionX,animationTime, true));
            _appearSequence.Insert(0,buttonsCanvas.DOFade(1,animationTime));
        }
        else
        {
            if (appearAnimation != null)
                appearAnimation.Play();
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

    private bool IsAppearAnimationWithUnityAnimation()
    {
        return !appearWithTween;
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

    protected virtual void OnDestroy()
    {
        tokenSource.Cancel();
    }
}