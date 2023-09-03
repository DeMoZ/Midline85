using System.Threading.Tasks;
using I2.Loc;
using TMPro;
using UniRx;
using UnityEngine;

namespace UI
{
    public class StatisticsView : AaWindow
    {
        public struct Ctx
        {
            public ReactiveCommand OnClickMenuButton;
            public ReactiveCommand OnClickNextLevelButton;
        }

        [SerializeField] private TMP_Text title = default;
        [SerializeField] private AaMenuButton menuButton = default;
        [SerializeField] private AaMenuButton nextLevelButton = default;

        [SerializeField] private TMP_Text textField = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            menuButton.onButtonClick.AddListener(OnClickMenu);
            nextLevelButton.onButtonClick.AddListener(OnClickNextLevel);
        }

        private void OnClickNextLevel() =>
            _ctx.OnClickNextLevelButton.Execute();

        private void OnClickMenu() =>
            _ctx.OnClickMenuButton.Execute();

        public async Task SetStatistic(StatisticsData data)
        {
            nextLevelButton.gameObject.SetActive(data.NextLevelExists);
            textField.text = new LocalizedString(data.EndKey);
            title.text = new LocalizedString(data.LevelKey);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            menuButton.onButtonClick.RemoveAllListeners();
            nextLevelButton.onButtonClick.RemoveAllListeners();
        }
    }
}