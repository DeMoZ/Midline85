using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu]
    public class GameEnds : ScriptableObject
    {
        public List<string> Ends = new List<string>();
    }
}