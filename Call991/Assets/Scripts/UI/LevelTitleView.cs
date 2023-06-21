using TMPro;
using UnityEngine;

namespace UI
{
    public class LevelTitleView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup = default;
        [SerializeField] private TextMeshProUGUI chapter = default;
        [SerializeField] private TextMeshProUGUI title = default;
        public CanvasGroup CanvasGroup => canvasGroup;

        public void Set(string chapter, string title)
        {
            this.chapter.text = chapter;
            this.title.text = title;
        }
    }
}