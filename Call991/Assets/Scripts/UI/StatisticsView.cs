using System.Collections.Generic;
using AaDialogueGraph;
using DG.Tweening;
using I2.Loc;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StatisticsView : AaWindow
    {
        public struct Ctx
        {
            public ReactiveCommand onClickMenuButton;
        }

        [SerializeField] private MenuButtonView menuButton = default;
        [SerializeField] private GameObject statisticsObjects = default;
        [SerializeField] private Image fadeImage = default;
        [Space] [SerializeField] private LocalizedString lockedTextKey = default;
        
        [Space] [SerializeField] private RectTransform table = default;
        [SerializeField] private StatisticsCellView cellPrefab = default;

        private Ctx _ctx;
        private Color fadeImageColor;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            menuButton.OnClick += OnClickMenu;

            fadeImageColor = fadeImage.color;
        }

        private void OnClickMenu() =>
            _ctx.onClickMenuButton.Execute();

        public void PopulateCells(List<RecordData> data)
        {
            foreach (Transform cell in table)
            {
                Destroy(cell.gameObject);
            }

            foreach (var record in data)
            {
                var cell = Instantiate(cellPrefab, table);
                cell.text.text = new LocalizedString(record.Key);
                
                var sprite = !string.IsNullOrEmpty(record.Sprite)
                    ? NodeUtils.GetObjectByPath<Sprite>(record.Sprite)
                    : null;
                
                cell.image.sprite = sprite;
            }
        }

        public void Fade(float time)
        {
            statisticsObjects.SetActive(false);
            fadeImageColor.a = 0;
            fadeImage.color = fadeImageColor;
            gameObject.SetActive(true);
            fadeImage.gameObject.SetActive(true);
            fadeImage.DOFade(1, time / 2).OnComplete(() => OnBlackScreenOn(time));
        }

        private void OnBlackScreenOn(float time)
        {
            statisticsObjects.SetActive(true);
            fadeImage.DOFade(0, time / 2).OnComplete(() => OnBlackScreenOff());
        }

        private void OnBlackScreenOff()
        {
            fadeImage.gameObject.SetActive(false);
        }
    }
}