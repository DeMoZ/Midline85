using System.Collections;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiSwitchScene : MonoBehaviour
    {
        public struct Ctx
        {
            public ReactiveProperty<string> onLoadingProcess;
            public bool toLevelScene;
            public bool firstLoad;
            public Blocker blocker;
        }

        [SerializeField] private TextMeshProUGUI loadingValue = default;
        [SerializeField] private GameObject loadingUi = default;
        [SerializeField] private CanvasGroup canvasGroup = default;
        [SerializeField] private float appearDelay = 0.05f;
        [SerializeField] private float appearDuration = 0.3f;
        
        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            OnLoadingProcess("0");
            _ctx.onLoadingProcess.Subscribe(OnLoadingProcess);
            
            loadingUi.SetActive(!ctx.toLevelScene);
            _ctx.blocker.EnableScreenFade(ctx.firstLoad);
        }

        private void OnEnable()
        {
            StartCoroutine(DelayAppear());
        }
            
        private IEnumerator DelayAppear()
        {
            canvasGroup.alpha = 0;
            yield return new WaitForSeconds(appearDelay);
            canvasGroup.DOFade(1, appearDuration).SetEase(Ease.InQuad);
        }
        
        private void OnLoadingProcess(string value)
        {
            loadingValue.text = value;
        }
    }
}