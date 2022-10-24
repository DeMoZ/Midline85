using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenuCredits : MonoBehaviour
    {
        public struct Ctx
        {
            public ReactiveCommand onClickToMenu;
        }

        [SerializeField] private MenuButtonView returnBtn = default;

        private Ctx _ctx;
        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        private void Awake()
        {
            returnBtn.OnClick += OnClickToMenu;
        }
        
        public void OnClickToMenu()
        {
            _ctx.onClickToMenu?.Execute();
        }
        
        private void OnDestroy()
        {
            returnBtn.OnClick -= OnClickToMenu;
        }
    }
}