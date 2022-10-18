using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StatisticsView : MonoBehaviour
    {
        public struct Ctx
        {
            public ReactiveCommand onClickMenuButton;
        }

        [SerializeField] private MenuButtonView menuButton = default;
        [SerializeField] private GameObject statisticsObjects = default;
        [SerializeField] private Image fadeImage = default;
        [Space] [SerializeField] private LocalizedString lockedTextKey = default;

        [Space] [SerializeField] private List<StatisticsCellView> cells = default;

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

        public void PopulateCells(List<StatisticElement> statisticElements)
        {
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (statisticElements.Count > i && statisticElements[i].isReceived)
                {
                    cell.image.sprite = statisticElements[i].sprite;
                    cell.text.text = statisticElements[i].isReceived ? statisticElements[i].description : lockedTextKey;
                    cell.arrow.SetActive(statisticElements[i].isReceived);
                    cell.gameObject.SetActive(true);
                }
                else
                {
                    cell.gameObject.SetActive(false);
                }
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