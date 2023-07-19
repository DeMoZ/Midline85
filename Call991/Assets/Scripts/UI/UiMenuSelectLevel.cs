using Configs;
using UnityEngine;

namespace UI
{
    public class UiMenuSelectLevel : AaWindow
    {
        public struct Ctx
        {
            public GameSet GameSet;
            public PlayerProfile Profile;
        }
        
        [SerializeField] private MenuButtonView menuButtonPrefab;
        [SerializeField] private RectTransform buttonsParent;
        
        private Ctx _ctx;

        // every time the screen is shown i need to repopulate the levels buttons with correct state
        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Populate()
        {
            foreach (Transform child in buttonsParent) 
                Destroy(child.gameObject);
            
            foreach (var level in _ctx.GameSet.GameLevels.Levels)
            {
                var btn = Instantiate(menuButtonPrefab, buttonsParent);
                //btn.OnClick 
            }
        }
    }
}