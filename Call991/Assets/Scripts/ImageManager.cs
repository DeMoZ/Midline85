using System.Collections.Generic;
using AaDialogueGraph;
using UnityEngine;
using UnityEngine.UI;

public class ImageManager : MonoBehaviour
{
    public struct Ctx
    {
        
    }

    [SerializeField] private List<Image> images = default;
    
    private Ctx _ctx;

    public void SetCtx(Ctx ctx)
    {
        _ctx = ctx;
    }

    public void ShowImage(EventVisualData data, Sprite sprite)
    {
        var layer = Mathf.Clamp((int)data.Layer, 0, images.Count - 1);
        var image = images[layer];

        if (data.Stop)
        {
            image.gameObject.SetActive(false);
            image.sprite = null;
        }
        else
        {
            Debug.Log($"[{this}] Stop videoPlayer {image.gameObject.name}");
            image.sprite = sprite;
            image.preserveAspect = true;
            image.gameObject.SetActive(true);
        }
    }
}