using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace UI
{
    public class LevelView : AbstractMultiControlComponentsWindow
    {
        public struct Ctx
        {
            public ReactiveCommand onClickPauseButton;
        }
        
        [SerializeField] private MenuButtonView pauseButton = default;
        [SerializeField] private List<ChoiceButtonView> buttons = default;
        [SerializeField] private List<PersonView> persons = default;
        [SerializeField] private  CountDownView countDown = default;
        [SerializeField] private CanvasGroup canvasGroup = default;
        
        public List<ChoiceButtonView> Buttons => buttons;

        public CountDownView CountDown => countDown;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            
            foreach (var person in persons)
            {
                person.gameObject.SetActive(false);
            }

            foreach (var button in buttons)
                button.gameObject.SetActive(false);

            CountDown.gameObject.SetActive(false);
            
            pauseButton.OnClick += OnClickPauseButton;
        }

        public void OnClickPauseButton()
        {
            _ctx.onClickPauseButton?.Execute();
        }
        
        public void OnShowPhrase(PhraseSet phrase)
        {
            var personView = persons.FirstOrDefault(p => p.ScreenPlace == phrase.screenPlace);
            if (personView == null)
            {
                Debug.LogError($"[{this}] [OnShowPhrase] no person on side {phrase.screenPlace}");
                return;
            }

            personView.ShowPhrase(phrase);
        }

        public void OnHidePhrase(PhraseSet phrase)
        {
            var personView = persons.FirstOrDefault(p => p.ScreenPlace == phrase.screenPlace);
            if (personView == null)
            {
                Debug.LogError($"[{this}] [OnHidePhrase] no person on side {phrase.screenPlace}");
                return;
            }

            if (phrase.hidePhraseOnEnd)
                personView.HidePhrase();

            if (phrase.hidePersonOnEnd)
                personView.gameObject.SetActive(false);
        }

        public void OnHideLevelUi(float time, Action callback)
        {
            Debug.LogWarning($"[{this}] OnHideLevelUi");
            canvasGroup.DOFade(0, time).OnComplete(() => { callback?.Invoke();});
        }
        
        public void Dispose()
        {
            pauseButton.OnClick -= OnClickPauseButton;
        }
    }
}