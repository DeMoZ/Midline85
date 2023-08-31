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
    private class AnimationSettings
    {
        public RectTransform ButtonsGroup = default;
        public CanvasGroup ButtonsCanvas = default;
        public PositionXAnimationConfig Config = default;
    }
    
    [Space] [SerializeField] private bool useAppearAnimation;

    [ShowIf("useAppearAnimation")] [SerializeField]
    private AnimationSettings appearAnimation;

    [Space] [SerializeField] private bool useDisappearAnimation;

    [ShowIf("useDisappearAnimation")] [SerializeField]
    private AnimationSettings disappearAnimation;

    [Space] [Space] [SerializeField] private AaSelectable[] windowSelectables = default;

    private Sequence _animationSequence;

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

    protected virtual void OnDisable()
    {
        foreach (var selectable in windowSelectables)
        {
            selectable.OnSelectObj -= OnSelectObj;
            selectable.OnUnSelect -= OnUnSelect;
        }
    }

    protected void AnimateDisappear(Action callback)
    {
        ResetAnimationSequence();

        var position = disappearAnimation.ButtonsGroup.position;
        position.x = disappearAnimation.Config.FromPositionX;
        disappearAnimation.ButtonsGroup.position = position;

        disappearAnimation.ButtonsCanvas.alpha = 1;

        _animationSequence.Append(disappearAnimation.ButtonsGroup.DOMoveX(disappearAnimation.Config.ToPositionX,
            disappearAnimation.Config.AnimationTime, true));
        _animationSequence.Insert(0,
                disappearAnimation.ButtonsCanvas.DOFade(0, disappearAnimation.Config.AnimationTime))
            .OnComplete(callback.Invoke);
    }

    private void AnimateAppear()
    {
        ResetAnimationSequence();

        var position = appearAnimation.ButtonsGroup.position;
        position.x = appearAnimation.Config.FromPositionX;
        appearAnimation.ButtonsGroup.position = position;
        appearAnimation.ButtonsCanvas.alpha = 0;
        _animationSequence.Append(appearAnimation.ButtonsGroup.DOMoveX(appearAnimation.Config.ToPositionX,
            appearAnimation.Config.AnimationTime, true));
        _animationSequence.Insert(0, appearAnimation.ButtonsCanvas.DOFade(1,
            appearAnimation.Config.AnimationTime));
    }

    private void ResetAnimationSequence()
    {
        _animationSequence?.Kill();
        _animationSequence = DOTween.Sequence();
        _animationSequence.SetUpdate(true);
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