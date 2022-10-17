using UnityEngine;

namespace UI
{
    public class LevelView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup = default;
        public CanvasGroup CanvasGroup => canvasGroup;
    }
}