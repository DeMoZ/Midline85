using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/LevelKeysList")]
    public class LevelKeysList : ScriptableObject
    {
        [SerializeField] protected List<string> keys;
        public virtual List<string> Keys => keys;
    }
}