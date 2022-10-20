using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class YieldUpdateHorizontalUseChildScale : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup layoutGroup = default;
    [SerializeField] private CanvasGroup canvasGroup = default;
    private void Start()
    {
        StartCoroutine(YieldNullAndForceTransform());
    }

    private IEnumerator YieldNullAndForceTransform()
    {
        canvasGroup.alpha = 0;
        yield return null;
        layoutGroup.childScaleWidth = true;
        canvasGroup.alpha = 1;
    }
}