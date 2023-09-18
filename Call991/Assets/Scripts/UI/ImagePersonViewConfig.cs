using UnityEngine;

[CreateAssetMenu(fileName = nameof(ImagePersonViewConfig), menuName = "Aa/Configs/" + nameof(ImagePersonViewConfig), order = 0)]
public class ImagePersonViewConfig : ScriptableObject
{
    [SerializeField] private float fadeTime = 0.7f;
    [SerializeField] private float scaleValue = 0.85f;
    [SerializeField] private float moveValue = 250f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color shadeColor = Color.gray;
    
    public float FadeTime => fadeTime;
    public float ScaleValue => scaleValue;
    public float MoveValue => moveValue;

    public Color NormalColor => normalColor;

    public Color ShadeColor => shadeColor;
}