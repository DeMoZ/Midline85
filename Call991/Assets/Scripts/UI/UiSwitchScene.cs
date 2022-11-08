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
        [SerializeField] private GameObject loadingTitle = default;
        
        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            OnLoadingProcess("0");
            _ctx.onLoadingProcess.Subscribe(OnLoadingProcess);
            
            loadingUi.SetActive(!ctx.toLevelScene);
            loadingTitle.SetActive(ctx.toLevelScene);
            _ctx.blocker.EnableScreenFade(ctx.firstLoad);
        }

        private void OnLoadingProcess(string value)
        {
            loadingValue.text = value;
        }
    }
}