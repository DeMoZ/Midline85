using UnityEngine;

public class BlinkingCanvasGroup : MonoBehaviour
{
   [SerializeField] private CanvasGroup canvasGroup;
   [SerializeField] private float duration = 1f;

   private void Update()
   {
      var currentTime = Time.time;
      var alpha = Mathf.PingPong(currentTime, duration);
      canvasGroup.alpha = alpha;
   }
}
