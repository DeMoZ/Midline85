using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Configs;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiLevelScene : MonoBehaviour, IDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand onClickMenuButton;
            public ReactiveCommand<string> onPhraseEvent;
            public ReactiveCommand<Phrase> onShowPhrase;
            public ReactiveCommand<Phrase> onHidePhrase;

            public GameSet gameSet;
            public Pool pool;
        }

        private const float FADE_TIME = 0.3f;

        private Ctx _ctx;

        [SerializeField] private Button menuButton = default;
        [SerializeField] private List<PersonView> persons;
        [SerializeField] private List<ChoiceButtonView> buttons;

        private CompositeDisposable _disposables;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            menuButton.onClick.AddListener(() => { _ctx.onClickMenuButton.Execute(); });

            _ctx.onPhraseEvent.Subscribe(OnPhraseEvent).AddTo(_disposables);
            _ctx.onShowPhrase.Subscribe(OnShowPhrase).AddTo(_disposables);
            _ctx.onHidePhrase.Subscribe(OnHidePhrase).AddTo(_disposables);

            foreach (var person in persons) 
                person.gameObject.SetActive(false);
            
            foreach (var button in buttons) 
                button.gameObject.SetActive(false);
        }

        private void OnPhraseEvent(string eventId)
        {
            
        }
        
        private void OnShowPhrase(Phrase phrase)
        {
            var personView = persons.FirstOrDefault(p => p.ScreenPlace == phrase.screenPlace);
            if (personView == null)
            {
                Debug.LogError($"[{this}] [OnShowPhrase] no person on side {phrase.screenPlace}");
                return;
            }

            personView.ShowPhrase(phrase);
        }

        private void OnHidePhrase(Phrase phrase)
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

        private async void OnShowDialog(Phrase phrase)
        {
            await Task.Yield();
            // show name
            var place = persons.First(d => d.ScreenPlace == phrase.screenPlace);
            place.gameObject.SetActive(true);
            //place.PersonName(phrase.person.ToString());// TODO Get Name

            // show text with effect Pop|letter|words
        }

        private async void OnHideDialog()
        {
            await Task.Yield();
        }

        private async void OnShowChoices()
        {
            await Task.Yield();
        }

        public void Dispose()
        {
        }
    }
}

/*// private async void OnShowButtons()
// {
//     if (_currentOperations.Count > 0)
//         await HideOperations();
//     
//     ShowOperations();
// }
//
// private void ShowOperations()
// {
//     // _interactionBtnsCanvasGroup.alpha = 0;
//     // _interactionBtnsCanvasGroup.DOFade(1, FADE_TIME);
// }
//
// private async Task HideOperations()
// {
//     // _interactionBtnsCanvasGroup.DOFade(0, FADE_TIME);
//
//     await Task.Delay((int) (FADE_TIME * 1000));
//
//     foreach (var btn in _currentOperations)
//     {
//         _ctx.pool.Return(btn.gameObject);
//         btn.onClick.RemoveAllListeners();
//     }
//
//     _currentOperations.Clear();
// }*/


/*
 From Test
 public class Dialogue
{
    public Piece leftPerson;
    public Piece rightPerson;
    public float dialogueDuration;
    public float decisionDuration;
    public List<Decision> decisions;
}

public class Decision
{
    public int index;
    public string description;
}

public class Piece
{
    public string name;
    public string text;
    public DialogAppearType appearType;
}*/