using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using I2.Loc;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenuSelectLevel : AaWindow
    {
        public struct Ctx
        {
            public GameLevelsService GameLevelsService;
            public ReactiveCommand<int> OnLevelSelect;
            public ReactiveCommand<int> OnLevelPlay;
            public ReactiveCommand OnClickToMenu;
        }

        [SerializeField] private AaMenuProgressButton levelButtonPrefab;
        [SerializeField] private RectTransform buttonsParent;
        [SerializeField] private AaMenuButton toMenuHintBtn = default;

        private Ctx _ctx;
        private List<AaButton> _buttons;
        private LocalizedString _localize;
        private List<string> loadingIds = new List<string>();

        // every time the screen is shown i need to repopulate the levels buttons with correct state
        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            toMenuHintBtn.onButtonClick.AddListener(OnClickToMenu);
        }

        public void OnClickToMenu()
        {
            _ctx.OnClickToMenu.Execute();
        }

        public void Populate()
        {
            foreach (Transform child in buttonsParent)
                Destroy(child.gameObject);

            var progressData = _ctx.GameLevelsService.DialogueLogger.LoadLevelsInfo();
            _buttons = new List<AaButton>();
            int lastFinished = -1;

            var levels = _ctx.GameLevelsService.GetLevels();
            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];

                var btn = Instantiate(levelButtonPrefab, buttonsParent);
                _localize = level.name;
                btn.Text = _localize;
                var info = progressData.FirstOrDefault(l => l.Key == level.name);

                if (info != null)
                {
                    if (info.HasRecord)
                    {
                        lastFinished = i;
                        btn.SetNormal();
                    }
                    else if (lastFinished == i - 1)
                    {
                        btn.SetNormal();
                    }
                    else
                    {
                        SetButtonDisabled(btn);
                    }
                }
                else
                {
                    SetButtonDisabled(btn);
                }

                var index = i;

                btn.onButtonSelect.AddListener(() => OnLevelSelect(index));
                btn.onButtonClick.AddListener(() => OnLevelClick(index));

                _buttons.Add(btn);
            }

            firstSelected = _buttons[0];
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var button in _buttons)
            {
                button.onButtonSelect.RemoveAllListeners();
                button.onButtonClick.RemoveAllListeners();
            }
            
            toMenuHintBtn.onButtonClick.RemoveAllListeners();
        }

        private void SetButtonDisabled(AaButton btn)
        {
#if !UNITY_EDITOR
//            btn.SetDisabled();
#endif
        }

        private void OnLevelClick(int index)
        {
            OnLevelClickAsync(index).Forget();
        }

        private async Task OnLevelClickAsync(int index)
        {
            // todo roman check for content loaded for the level and if not, download
            var levelId = _ctx.GameLevelsService.GetLevelId(index);
            if (loadingIds.Contains(levelId))
                return;

            loadingIds.Add(levelId);
            var tokenSource = new CancellationTokenSource();
            var isDownloaded = await _ctx.GameLevelsService.AddressableDownloader.IsDownloadedAsync(levelId, tokenSource.Token);

            ((AaMenuProgressButton)_buttons[index]).EnableProgress(!isDownloaded);
            if (!isDownloaded)
            {
                await _ctx.GameLevelsService.AddressableDownloader.DownloadAsync<Object>(levelId, value => OnProgress(value, index), tokenSource.Token);
                return;
            }

            return;
            AnimateDisappear(() =>
            {
               // Debug.LogWarning($"Level {index} clicked");
                _ctx.OnLevelPlay?.Execute(index);
            });
        }

        private void OnProgress(float value, int index)
        {
            if (!gameObject.activeInHierarchy)
                return;

            var button = (AaMenuProgressButton)_buttons[index];
            button.SetProgress(value);
        }

        private void OnLevelSelect(int index)
        {
            //Debug.LogWarning($"Level {index} selected");
            // todo roman check for content loaded for the level and if not, dont animate
            //AddressableDownloader.
            _ctx.OnLevelSelect?.Execute(index);
        }
    }
}