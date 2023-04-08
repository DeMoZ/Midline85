using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/PopupKeysList")]
    public class PopupKeysList : ScriptableObject
    {
        [SerializeField] protected List<string> keys;
        public virtual List<string> Keys => keys;
    }
}