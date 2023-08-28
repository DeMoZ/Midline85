using DG.Tweening;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu]
    public class ScaleSet : ScriptableObject
    {
        [SerializeField] private float fromScale = 1f;
        [SerializeField] private float toScale = 1.2f;
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private Ease ease = Ease.OutBack;

        public float FromScale => fromScale;
        public float ToScale => toScale;
        public float Duration => duration;
        public Ease Ease => ease;

    }
}