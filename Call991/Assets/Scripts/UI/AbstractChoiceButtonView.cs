using UniRx;
using UnityEngine.UI;

namespace UI
{
    public abstract class AbstractChoiceButtonView : Selectable
    {
        public struct Ctx
        {
            public int index;
            public ReactiveCommand<int> onClickChoiceButton;
            public float buttonsAppearDuration;
            public float fastButtonFadeDuration;
            public float slowButtonFadeDuration;
        }

        protected Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        public abstract void Show(string choiceId, bool isBlocked);

        public abstract void Choose();
    }
}