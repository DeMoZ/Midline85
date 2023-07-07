using System.Collections.Generic;
using AK.Wwise;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/WwiseRtpcList", fileName = "RtpcList")]
    public class WwiseRtpcList : ScriptableObject
    {
        [SerializeField] private List<AK.Wwise.RTPC> wwiseRtpcs = new ();

        public List<RTPC> WwiseRtpcs => wwiseRtpcs;
        
        public virtual List<string> Keys => GetKeys();

        [Button("Test See Keys")]
        public List<string> GetKeys()
        {
            var result = new List<string> { AaGraphConstants.None };

            foreach (var rtpc in wwiseRtpcs)
            {
                result.Add(rtpc.Name);
            }

            return result;
        }
    }
}