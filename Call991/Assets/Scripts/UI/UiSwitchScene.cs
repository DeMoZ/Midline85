using System.Threading.Tasks;
using Configs;
using Data;
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
            public PhraseEventVideoLoader phraseEventVideoLoader;
            public GameSet gameSet;
        }
    
        [SerializeField] private TextMeshProUGUI loadingValue = default;
        [SerializeField] private GameObject loadingUi = default;
        [SerializeField] private GameObject loadingTitle = default;

        private Ctx _ctx;
        
        public async Task SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            OnLoadingProcess("0");
            _ctx.onLoadingProcess.Subscribe(OnLoadingProcess);

            if (_ctx.toLevelScene) 
                await _ctx.phraseEventVideoLoader.LoadVideoTitle(_ctx.gameSet.titleVideoSoName);

            loadingUi.SetActive(!ctx.toLevelScene);
            loadingTitle.SetActive(ctx.toLevelScene);
        }

        private void OnLoadingProcess(string value)
        {
            loadingValue.text = value;
        }
    }
}
