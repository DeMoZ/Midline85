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
        [SerializeField] private MenuButtonView menuButton = default;
        [SerializeField] private MenuButtonView nextLevelButton = default;

        [Space] [SerializeField] private RectTransform table = default;

        [SerializeField] private TMP_Text textField = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            menuButton.OnClick += OnClickMenu;
            nextLevelButton.OnClick += OnClickNextLevel;
        }

        private void OnClickNextLevel() => 
            _ctx.OnClickNextLevelButton.Execute();

        private void OnClickMenu() =>
            _ctx.OnClickMenuButton.Execute();

        public async Task SetStatistic(StatisticsData data)
        {   
            nextLevelButton.gameObject.SetActive(data.NextLevelExists);
            textField.text = new LocalizedString(data.EndKey);
            title.text =  new LocalizedString(data.LevelKey);
        }
        
        // public async Task _PopulateCells(List<RecordData> data, bool nextLevelExists)
        // {   
        //     nextLevelButton.gameObject.SetActive(nextLevelExists);
        //     
        //     foreach (Transform cell in table) 
        //         Destroy(cell.gameObject);
        //
        //     foreach (var record in data)
        //     {
        //         var cell = Instantiate(cellPrefab, table);
        //         cell.text.text = new LocalizedString(record.Key);
        //
        //         Sprite sprite;
        //         if (!string.IsNullOrEmpty(record.Sprite))
        //         {
        //             sprite = await NodeUtils.GetObjectByPathAsync<Sprite>(record.Sprite);
        //             if (tokenSource is { IsCancellationRequested: true }) return;
        //         }
        //         else
        //         {
        //             sprite = null;
        //         }
        //
        //         cell.image.sprite = sprite;
        //     }
        // }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            menuButton.OnClick -= OnClickMenu;
        }
    }
}
