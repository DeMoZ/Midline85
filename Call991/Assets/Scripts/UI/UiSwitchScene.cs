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
        }
    
        [SerializeField] private TextMeshProUGUI loadingValue = default;
        [SerializeField] private GameObject loadingTitle = default;

        public void SetCtx(Ctx ctx)
        {
            OnLoadingProcess("0");
            ctx.onLoadingProcess.Subscribe(OnLoadingProcess);
            loadingTitle.SetActive(ctx.toLevelScene);
        }

        private void OnLoadingProcess(string value)
        {
            loadingValue.text = value;
        }
    }
}
