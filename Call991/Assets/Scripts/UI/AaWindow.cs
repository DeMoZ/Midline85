using System;
using System.Threading;
using DG.Tweening;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class AaWindow : InputHandler
{
    [Serializable]
    private class AppearAnimation
    {
        public RectTransform ButtonsGroup = default;
        public CanvasGroup ButtonsCanvas = default;
        public float AnimationTime = 0.5f;
        public float FromPositionX = 0; 
        public float ToPositionX = 234f;
    }

    [Space] [SerializeField] private bool useAppearAnimation;
    [ShowIf("useAppearAnimation")]
    [SerializeField] private AppearAnimation appearAnimation;

    [Space] [Space] [SerializeField] private AaSelectable[] windowSelectables = default;

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

        if (useAppearAnimation)
            AnimateAppear();
    }

    private void AnimateAppear()
    {
        _appearSequence?.Kill();
        _appearSequence = DOTween.Sequence();

        _appearSequence.SetUpdate(true);
        var position = appearAnimation.ButtonsGroup.position;
        position.x = appearAnimation.FromPositionX;
        appearAnimation.ButtonsGroup.position = position;
        appearAnimation.ButtonsCanvas.alpha = 0;
        _appearSequence.Append(appearAnimation.ButtonsGroup.DOMoveX(appearAnimation.ToPositionX,
            appearAnimation.AnimationTime, true));
        _appearSequence.Insert(0, appearAnimation.ButtonsCanvas.DOFade(1, appearAnimation.AnimationTime));
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

    protected virtual void OnDestroy()
    {
        tokenSource.Cancel();
    }
}