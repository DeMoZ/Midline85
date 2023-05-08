using System.Collections.Generic;
using System.Threading.Tasks;
using AaDialogueGraph;
using I2.Loc;
using UniRx;
using UnityEngine;

namespace UI
{
    public class StatisticsView : AaWindow
    {
        public struct Ctx
        {
            public ReactiveCommand OnClickMenuButton;
            public Blocker Blocker;
        }

        [SerializeField] private MenuButtonView menuButton = default;

        [Space] [SerializeField] private RectTransform table = default;
        [SerializeField] private StatisticsCellView cellPrefab = default;

        private Ctx _ctx;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            menuButton.OnClick += OnClickMenu;
        }

        private void OnClickMenu() =>
            _ctx.OnClickMenuButton.Execute();

        public async Task PopulateCells(List<RecordData> data)
        {
            foreach (Transform cell in table)
            {
                Destroy(cell.gameObject);
            }

            foreach (var record in data)
            {
                var cell = Instantiate(cellPrefab, table);
                cell.text.text = new LocalizedString(record.Key);

                Sprite sprite;
                if (!string.IsNullOrEmpty(record.Sprite))
                {
                    sprite = await NodeUtils.GetObjectByPathAsync<Sprite>(record.Sprite);
                    if (tokenSource is { IsCancellationRequested: true }) return;
                }
                else
                    sprite = null;

                cell.image.sprite = sprite;
            }
        }
    }
}