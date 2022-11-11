using UnityEngine;

namespace UI
{
    public class LevelTitleView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup = default;
        public CanvasGroup CanvasGroup => canvasGroup;
    }
}