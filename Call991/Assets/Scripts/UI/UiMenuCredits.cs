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

        [SerializeField] private DevelopersSo developers;
        [SerializeField] private DeveloperPerson developerPrefab;
        [SerializeField] private RectTransform developersParent;
        [SerializeField] private MenuButtonView returnBtn = default;

        private Ctx _ctx;
        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
        }

        private void Awake()
        {
            returnBtn.OnClick += OnClickToMenu;
            
            foreach (Transform developer in developersParent)
                Destroy(developer.gameObject);

            foreach (var developer in developers.Developers)
            {
                var developerPerson = Instantiate(developerPrefab, developersParent);
                developerPerson.Set(developer.Position,  developer.NameKey);
            }
        }
        
        public void OnClickToMenu()
        {
            _ctx.OnClickToMenu?.Execute();
        }
        
        private void OnDestroy()
        {
            returnBtn.OnClick -= OnClickToMenu;
        }
    }
}