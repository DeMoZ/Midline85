using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/WwiseMusicList", fileName = "MusicList")]
    public class WwiseMusicSwitchesList : ScriptableObject
    {
        [SerializeField] private List<AK.Wwise.Switch> wwiseSwitches = new ();

        public List<Switch> WwiseSwitches => wwiseSwitches;
    }
}