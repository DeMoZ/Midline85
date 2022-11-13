using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu]
    public class ButtonAudioSettings : SerializedScriptableObject
    {
        [SerializeField] private AudioClip buttonHoverClip = default;
        [SerializeField] private AudioClip buttonClickClip = default;
        
        private event Action<AudioClip> OnClick;
        public event Action<AudioClip> OnHover;

        public void PlayHoverSound()
        {
            if (!buttonHoverClip) return;
            
            OnHover?.Invoke(buttonHoverClip);
        }

        public void PlayClickSound()
        {
            if (!buttonClickClip) return;
            
            OnClick?.Invoke(buttonClickClip);
        }
    }
}