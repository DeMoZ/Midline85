using UnityEngine;

[CreateAssetMenu(fileName = nameof(ImagePersonViewConfig), menuName = "Aa/Configs/" + nameof(ImagePersonViewConfig), order = 0)]
public class ImagePersonViewConfig : ScriptableObject
{
    [SerializeField] private float fadeTime = 0.7f;
    [SerializeField] private float fadeValue = 0.45f;
    [SerializeField] private float scaleValue = 0.85f;
    [SerializeField] private float moveValue = 250f;

    public float FadeTime => fadeTime;
    public float FadeValue => fadeValue;
    public float ScaleValue => scaleValue;
    public float MoveValue => moveValue;
}