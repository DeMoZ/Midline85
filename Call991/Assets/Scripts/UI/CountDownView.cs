using System;
using System.Collections.Generic;
using AaDialogueGraph;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace UI
{
    public class CountDownView : MonoBehaviour
    {
        public struct Ctx
        {
            public float ChoicesDuration;
        }

        [SerializeField] private RectTransform backLine = default;
        [SerializeField] private RectTransform frontLine = default;

        private Ctx _ctx;
        private Tween _scaleTween;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Stop()
        {
            _scaleTween.Kill();
        }

        public void Show()
        {
            frontLine.sizeDelta = backLine.sizeDelta;
            gameObject.SetActive(true);
            _scaleTween = frontLine.DOSizeDelta(Vector2.up, _ctx.ChoicesDuration);
        }

        private void OnTweenUpdate()
        {
            Debug.Log($"{_scaleTween.Elapsed()}");
        }
    }
}