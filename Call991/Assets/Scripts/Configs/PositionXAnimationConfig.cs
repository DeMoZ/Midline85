using UnityEngine;

[CreateAssetMenu(fileName = nameof(PositionXAnimationConfig), menuName = "Aa/Configs/" + nameof(PositionXAnimationConfig), order = 0)]
public class PositionXAnimationConfig : ScriptableObject
{
    [SerializeField] private float animationTime = 0f;
    [SerializeField] private float fromPositionX = 0; 
    [SerializeField] private float toPositionX = 0f;

    public float AnimationTime => animationTime;
    public float FromPositionX => fromPositionX;
    public float ToPositionX => toPositionX;
}