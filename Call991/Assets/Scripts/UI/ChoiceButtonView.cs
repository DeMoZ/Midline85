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
            public float fastButtonFadeDuration;
            public float slowButtonFadeDuration;
        }
        
        [SerializeField] private Button button = default;
        [SerializeField] private TextMeshProUGUI text = default;
        [SerializeField] private CanvasGroup canvasGroup = default;
        
        private Ctx _ctx;

        public Button Button => button;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            
            button.onClick.AddListener(() => _ctx.onClickChoiceButton.Execute(_ctx.index));
        }

        public async void Show(string description)
        {
            text.text = description;
            gameObject.SetActive(true);
            canvasGroup.DOFade(1, _ctx.slowButtonFadeDuration);
            await Task.Delay((int)(_ctx.slowButtonFadeDuration * 1000));
        }

        public async void Hide(bool slow)
        {
            var duration = slow ? _ctx.slowButtonFadeDuration : _ctx.fastButtonFadeDuration;
            canvasGroup.DOFade(0, duration);
            await Task.Delay((int)(duration * 1000));
            
            gameObject.SetActive(false);
        }
    }
}