using UnityEngine;
using UnityEngine.UI;

namespace Configs
{
    [CreateAssetMenu]
    public class GameSet : ScriptableObject
    {
        public GameObject table = default;
        public Button buttonPrefab = default;
    }
    
}