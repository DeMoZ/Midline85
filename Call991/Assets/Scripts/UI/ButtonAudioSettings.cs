using System;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu]
    public class ButtonAudioSettings : ScriptableObject
    {
        [SerializeField] private AK.Wwise.Event buttonHoverSound;
        [SerializeField] private AK.Wwise.Event buttonClickSound;
        
        public event Action<AK.Wwise.Event> OnClick;
        public event Action<AK.Wwise.Event> OnHover;

        public void PlayHoverSound()
        {
            OnHover?.Invoke(buttonHoverSound);
        }

        public void PlayClickSound()
        {
            OnClick?.Invoke(buttonClickSound);
        }
    }
}