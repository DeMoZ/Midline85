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
        
        public bool TryGetRtpcByName(string sName, out RTPC wRtpc)
        {
            foreach (var element in wwiseRtpcs)
            {
                if (element.Name.Equals(sName))
                {
                    wRtpc = element;
                    return true;
                }
            }

            wRtpc = null;
            return false;
        }
    }
}