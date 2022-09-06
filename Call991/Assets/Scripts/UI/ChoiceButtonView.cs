using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChoiceButtonView : MonoBehaviour
    {
        public struct Ctx
        {
            public int index;
            public ReactiveCommand<int> onClickChoiceButton;
            public float buttonsAppearDuration;
            public float fastButtonFadeDuration;
            public float slowButtonFadeDuration;
        }

        [SerializeField] private GameObject clickedImage = default;
        [SerializeField] private Button button = default;
        [SerializeField] private TextMeshProUGUI clickedText = default;
        [SerializeField] private TextMeshProUGUI buttonText = default;
        [SerializeField] private CanvasGroup canvasGroup = default;
        
        private Ctx _ctx;

        public Button Button => button;
        private static bool _isClicked;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            
            button.onClick.AddListener(() => OnClick());
        }

        private void OnClick()
        {
            if (_isClicked) return;

            _isClicked = true;
            _ctx.onClickChoiceButton.Execute(_ctx.index);
        }

        public void SetClicked()
        {
            clickedImage.gameObject.SetActive(true);
            button.gameObject.SetActive(false);
        }
        
        public async void Show(string description)
        {
            _isClicked = false;
            
            clickedText.text = description;
            buttonText.text = description;
            
            clickedImage.gameObject.SetActive(false);
            button.gameObject.SetActive(true);
            
            gameObject.SetActive(true);
            canvasGroup.DOFade(1, _ctx.buttonsAppearDuration);
            await Task.Delay((int)(_ctx.buttonsAppearDuration * 1000));
        }

        public async void Hide(bool slow)
        {
            var duration = slow ? _ctx.slowButtonFadeDuration : _ctx.fastButtonFadeDuration;
            canvasGroup.DOFade(0, duration);
            await Task.Delay((int)(duration * 1000));
        }
    }
}