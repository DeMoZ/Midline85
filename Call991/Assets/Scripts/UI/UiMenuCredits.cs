using System.Linq;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UiMenuCredits : MonoBehaviour
    {
        public struct Ctx
        {
            public ReactiveCommand OnClickToMenu;
        }

        [SerializeField] private DevelopersSo developers = default;
        [SerializeField] private RectTransform developerPanelPrefab = default;
        [SerializeField] private DevelopersView developersViewPrefab = default;
        [SerializeField] private RectTransform panels = default;
        [SerializeField] private AaMenuButton returnBtn = default;

        private Ctx _ctx;
        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            returnBtn.onButtonClick.AddListener(OnClickToMenu);
        }

        private void Awake()
        {
            foreach (Transform developer in panels)
                Destroy(developer.gameObject);

            foreach (var group in developers.Developers)
            {
                var devGroup = Instantiate(developerPanelPrefab, panels);

                foreach (var developer in group.developers)
                {
                    var developerView = Instantiate(developersViewPrefab, devGroup);
                    var names = developer.NameKeys.Select(str => (string)str).ToList();
                    developerView.Set(developer.Position,  names);
                }
            }
        }
        
        public void OnClickToMenu()
        {
            _ctx.OnClickToMenu?.Execute();
        }
        
        private void OnDestroy()
        {
            returnBtn.onButtonClick.RemoveAllListeners();
        }
    }
}