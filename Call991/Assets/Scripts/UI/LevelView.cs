using System;
using System.Collections.Generic;
using System.Linq;
using AaDialogueGraph;
using Configs;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LevelView : AaWindow
    {
        public struct Ctx
        {
            public GameSet GameSet;
            public ReactiveCommand OnClickPauseButton;
            public LevelSceneObjectsService LevelSceneObjectsService;
        }

        [SerializeField] private Button pauseButton = default;
        [SerializeField] private CanvasGroup choiceCanvasGroup;
        [SerializeField] private List<GameObject> buttonSplitters = default;
        [SerializeField] private List<PersonView> persons = default;
        [SerializeField] private List<ImagePersonView> imagePersons = default;
        [SerializeField] private CountDownView countDown = default;
        [SerializeField] private CanvasGroup canvasGroup = default;

        [SerializeField] private TextPersonView imagePersonText = default;
        
        private Ctx _ctx;
        private CompositeDisposable _disposables;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            countDown.SetCtx(new CountDownView.Ctx
            {
                ChoicesDuration = _ctx.GameSet.choicesDuration,
            });

            for (var i = 0; i < Buttons.Length; i++)
            {
                var button = (AaChoiceButton)Buttons[i];
                button.SetCtx(new AaChoiceButton.Ctx
                {
                    Index = i,
                    OnClickChoiceButton = _ctx.LevelSceneObjectsService.OnClickChoiceButton,
                    OnAutoSelectButton = _ctx.LevelSceneObjectsService.OnAutoSelectButton,
                });
            }

            foreach (var person in persons)
                person.gameObject.SetActive(false);

            foreach (var person in imagePersons)
                person.gameObject.SetActive(false);

            imagePersonText.gameObject.SetActive(false);
            HideButtons();
            
            _ctx.LevelSceneObjectsService.OnShowButtons.Subscribe(OnShowButtons).AddTo(_disposables);
            _ctx.LevelSceneObjectsService.OnHideButtons.Subscribe(_ => OnHideButtons()).AddTo(_disposables);
            _ctx.LevelSceneObjectsService.OnClickChoiceButton.Subscribe(_ => DisactiveButtonsStopTimer()).AddTo(_disposables);
            pauseButton.onClick.AddListener(OnClickPauseButton);
        }

        private void OnShowButtons(List<ChoiceNodeData> data)
        {
            HideButtons();
            choiceCanvasGroup.alpha = 0;
            if (data == null) return;
            if (data.Count > 0)
                buttonSplitters[0].gameObject.SetActive(true);

            for (var i = 0; i < data.Count; i++)
            {
                var button = (AaChoiceButton)Buttons[i];
                button.interactable = !data[i].IsLocked;
                button.Show(data[i].Choice, data[i].IsLocked, data[i].ShowUnlock);
                buttonSplitters[i + 1].gameObject.SetActive(true);
            }

            choiceCanvasGroup.DOFade(1, _ctx.GameSet.buttonsAppearDuration);
            countDown.Show();
        }

        private void DisactiveButtonsStopTimer()
        {
            foreach (var button in Buttons) 
                button.interactable = false;

            countDown.Stop();
        }

        /// <summary>
        /// on hide buttons event
        /// </summary>
        private void OnHideButtons()
        {
            choiceCanvasGroup.alpha = 1;
            choiceCanvasGroup.DOFade(0, _ctx.GameSet.buttonsDisappearDuration);
        }

        /// <summary>
        /// hide buttons before show new buttons goup
        /// </summary>
        private void HideButtons()
        {
            foreach (var button in Buttons)
                button.gameObject.SetActive(false);

            foreach (var splitter in buttonSplitters)
                splitter.gameObject.SetActive(false);

            countDown.gameObject.SetActive(false);
        }

        public void OnClickPauseButton()
        {
            _ctx.OnClickPauseButton?.Execute();
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
                PersonVisualData = new PersonVisualData { Person = data.PersonVisualData.Person },
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

        public void OnHideImagePhrase(UiImagePhraseData data)
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
            pauseButton.onClick.RemoveAllListeners();
        }
    }
}