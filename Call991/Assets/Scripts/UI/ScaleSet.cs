using DG.Tweening;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu]
    public class ScaleSet : ScriptableObject
    {
        [SerializeField] private float size = 1.2f;
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private Ease ease = Ease.OutBack;

        public float Size => size;
        public float Duration => duration;
        public Ease Ease => ease;
    }
}