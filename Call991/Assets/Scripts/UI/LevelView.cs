using System;
using System.Collections.Generic;
using System.Linq;
using AaDialogueGraph;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace UI
{
    public class LevelView : AaWindow
    {
        public struct Ctx
        {
            public ReactiveCommand onClickPauseButton;
        }

        [SerializeField] private MenuButtonView pauseButton = default;
        [SerializeField] private List<ChoiceButtonView> buttons = default;
        [SerializeField] private List<PersonView> persons = default;
        [SerializeField] private TextPersonView imagePersonText = default;
        [SerializeField] private List<ImagePersonView> imagePersons = default;
        [SerializeField] private CountDownView countDown = default;
        [SerializeField] private CanvasGroup canvasGroup = default;

        public List<ChoiceButtonView> Buttons => buttons;

        public CountDownView CountDown => countDown;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;

            foreach (var person in persons) 
                person.gameObject.SetActive(false);

            foreach (var person in imagePersons) 
                person.gameObject.SetActive(false);
            
            imagePersonText.gameObject.SetActive(false);

            foreach (var button in buttons)
                button.gameObject.SetActive(false);

            CountDown.gameObject.SetActive(false);

            pauseButton.OnClick += OnClickPauseButton;
        }

        public void OnClickPauseButton()
        {
            _ctx.onClickPauseButton?.Execute();
        }

        public void OnShowPhrase(UiPhraseData data)
        {
            var personView = persons.FirstOrDefault(p => p.ScreenPlace == data.PersonVisualData.ScreenPlace);
            if (personView == null)
            {
                Debug.LogError($"[{this}] [OnShowPhrase] no person on side {data.PersonVisualData.ScreenPlace}");
                return;
            }

            personView.ShowPhrase(data);
        }

        public void OnShowImagePhrase(UiImagePhraseData data)
        {
            var personView = imagePersons.FirstOrDefault(p => p.ScreenPlace == data.PersonVisualData.ScreenPlace);
            if (personView == null)
            {
                Debug.LogError($"[{this}] [OnShowPhrase] no person on side {data.PersonVisualData.ScreenPlace}");
                return;
            }
            
            personView.ShowPhrase(data);

            var phraseData = new UiPhraseData
            {
                Description = data.Description,
                Phrase = data.Phrase,
                PhraseVisualData = data.PhraseVisualData,
                PersonVisualData = new PersonVisualData{Person = data.PersonVisualData.Person},
            };
            imagePersonText.ShowPhrase(phraseData);
        }

        public void OnHidePhrase(UiPhraseData data)
        {
            var personView = persons.FirstOrDefault(p => p.ScreenPlace == data.PersonVisualData.ScreenPlace);
            if (personView == null)
            {
                Debug.LogError($"[{this}] [OnHidePhrase] no person on side {data.PersonVisualData.ScreenPlace}");
                return;
            }

            if (data.PhraseVisualData.HideOnEnd)
                personView.HidePhrase();

            if (data.PersonVisualData.HideOnEnd)
                personView.gameObject.SetActive(false);
        }
        
        public void OnHideImagePhrase (UiImagePhraseData data)
        {
            var personView = imagePersons.FirstOrDefault(p => p.ScreenPlace == data.PersonVisualData.ScreenPlace);
            if (personView == null)
            {
                Debug.LogError($"[{this}] [OnHidePhrase] no person on side {data.PersonVisualData.ScreenPlace}");
                return;
            }

            personView.HidePhrase();
        }

        public void OnHideLevelUi(float time, Action callback)
        {
            Debug.LogWarning($"[{this}] OnHideLevelUi");
            canvasGroup.DOFade(0, time).OnComplete(() => { callback?.Invoke(); });
        }

        public void Dispose()
        {
            pauseButton.OnClick -= OnClickPauseButton;
        }
    }
}