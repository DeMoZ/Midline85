using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu]
    public class PopupKeysList : ScriptableObject
    {
        public List<string> Keys = new ();
    }
}