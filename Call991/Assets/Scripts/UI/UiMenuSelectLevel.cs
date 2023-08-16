using System.Collections.Generic;
using System.Linq;
using Configs;
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
            public DialogueLoggerPm DialogueLogger;
            public ReactiveCommand<int> OnLevelSelect;
            public ReactiveCommand<int> OnLevelPlay;
            public ReactiveCommand OnClickToMenu;
        }

        [SerializeField] private MenuButtonView levelButtonPrefab;
        [SerializeField] private RectTransform buttonsParent;
        [SerializeField] private MenuButtonView toMenuTutorialBtn = default;

        private Ctx _ctx;
        private List<MenuButtonView> _buttons;
        private LocalizedString _localize;

        // every time the screen is shown i need to repopulate the levels buttons with correct state
        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            toMenuTutorialBtn.OnClick += OnClickToMenu;
        }

        public void OnClickToMenu()
        {
            _ctx.OnClickToMenu.Execute();
        }
        
        public void Populate()
        {
            foreach (Transform child in buttonsParent)
                Destroy(child.gameObject);

            var progressData = _ctx.DialogueLogger.LoadLevelsInfo();
            _buttons = new List<MenuButtonView>();
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

                _buttons.Add(btn);

                var index = i;
                btn.OnSelectObj += _ => OnLevelSelect(index);
                btn.OnClick += () => OnLevelClick(index);
            }
        }

        private void SetButtonDisabled(MenuButtonView btn)
        {
#if !UNITY_EDITOR
//            btn.SetDisabled();
#endif
        }

        private void OnLevelClick(int index)
        {
            Debug.LogWarning($"Level {index} clicked");
            _ctx.OnLevelPlay?.Execute(index);
        }

        private void OnLevelSelect(int index)
        {
            Debug.LogWarning($"Level {index} selected");
            _ctx.OnLevelSelect?.Execute(index);
        }
    }
}