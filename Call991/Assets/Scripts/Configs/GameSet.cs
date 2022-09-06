using UnityEngine;

namespace Configs
{
    [CreateAssetMenu]
    public class GameSet : ScriptableObject
    {
        public float choicesDuration = 3f;
        [Space]
        public float buttonsAppearDuration = 0.2f;
        public float fastButtonFadeDuration = 0.3f;
        public float slowButtonFadeDuration = 0.6f;
    }
}