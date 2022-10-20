using System.Collections;
using TMPro;
using UnityEngine;

public class YieldUpdateTransform : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(YieldNullAndForceTransform());
    }

    private IEnumerator YieldNullAndForceTransform()
    {
        yield return new WaitForSeconds(1f);

        var ch = GetComponentsInChildren<TextMeshProUGUI>();

        foreach (var t in ch)
        {
            t.text = t.text;
        }
        
         foreach (Transform child in transform) 
             (child as RectTransform).ForceUpdateRectTransforms();
        
         (transform as RectTransform).ForceUpdateRectTransforms();
    }
}