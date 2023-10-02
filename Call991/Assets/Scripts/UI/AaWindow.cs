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

    [Tooltip("Menu Buttons Animation anchor to get X position")]
    [Space] [SerializeField] private RectTransform anchor = default;
    [Space] [SerializeField] private AaButton[] buttons = default;

    private Sequence _animationSequence;
    private CancellationTokenSource _tokenSource;
    private float _anchorX;

    protected AaButton[] Buttons => buttons;
    
    private void Awake()
    {
        _tokenSource = new CancellationTokenSource();
        
        if(useAppearAnimation || useDisappearAnimation)
            _anchorX = anchor.position.x;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // reset any selected object in case
        EventSystem.current.firstSelectedGameObject = null;
        EventSystem.current.SetSelectedGameObject(null);
        
//        Debug.LogWarning($"<---------Start>");
        foreach (var button in Buttons)
        {
            var btn = button;
            button.onButtonSelect.AddListener(()=> OnButtonSelect(btn));
            button.onButtonNormal.AddListener(()=> OnButtonNormal(btn));

//            Debug.LogWarning($"{button.name} selected = {button.IsSelected};\n     KeyboardSelected {button.IsKeyboardSelected}; MouseSelected {button.IsMouseSelected}");
            // if (button.IsSelected && button.IsKeyboardSelected && !button.IsMouseSelected)
            // {
            //     button.SetNormal();
            //     EventSystem.current.firstSelectedGameObject = button.gameObject;
            // }
            
            button.Reset();
        }
        EventSystem.current.firstSelectedGameObject = null;
//        Debug.LogWarning($"<---------End>");
        
        if (useAppearAnimation)
            AnimateAppear();
    }

    protected virtual void OnDisable()
    {
        foreach (var button in Buttons)
        {
            button.onButtonSelect.RemoveAllListeners();
            button.onButtonNormal.RemoveAllListeners();
        }
    }

    protected void AnimateDisappear(Action callback)
    {
        if (!useDisappearAnimation) return;
        
        ResetAnimationSequence();

        var position = disappearAnimation.ButtonsGroup.position;
        position.x = _anchorX;;
        disappearAnimation.ButtonsGroup.position = position;

        disappearAnimation.ButtonsCanvas.alpha = 1;

        _animationSequence.Append(disappearAnimation.ButtonsGroup.DOMoveX(_anchorX * 2,
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
        _animationSequence.Append(appearAnimation.ButtonsGroup.DOMoveX(_anchorX,
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


    // private void OnUnSelect(AaSelectable obj)
    // {
    //     //Debug.Log($"[{this}] <color=red>Window</color> to OnUnSelect {obj.gameObject.ToStringEventSystem()}");
    //     firstSelected = obj;
    //     EventSystem.current.SetSelectedGameObject(null);
    // }
    //
    // private void OnSelectObj(AaSelectable obj)
    // {
    //     //Debug.Log($"[{this}] <color=red>Window</color> to OnSelect {obj.gameObject.ToStringEventSystem()}");
    //     firstSelected = obj;
    //
    //     if (!EventSystem.current.alreadySelecting)
    //         EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    // }
    
    private void OnButtonNormal(AaButton button)
    {
        //Debug.Log($"[{this}] <color=red>Window</color> to OnUnSelect {obj.gameObject.ToStringEventSystem()}");
        //firstSelected = button;
//        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnButtonSelect(AaButton button)
    {
        //Debug.Log($"[{this}] <color=red>Window</color> to OnSelect {obj.gameObject.ToStringEventSystem()}");
        firstSelected = button;

        if (!EventSystem.current.alreadySelecting)
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    }

    protected virtual void OnDestroy()
    {
        _tokenSource.Cancel();
    }
}