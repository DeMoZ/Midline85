using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(menuName = "AaDialogueGraph/PersonKeysList")]
    public class PersonKeysList : ScriptableObject
    {
        [SerializeField] protected List<PersonName> keys;
        public virtual List<string> Keys => keys.Select(key => key.Name).ToList();
        // public virtual List<Color> Colors => keys.Select(key => key.Color).ToList();
        // public virtual List<string> ColorKeys => keys.Select(key => $"<color={key.Color}>{key.Name}</color>").ToList();

        public string GetColorKey(string key)
        {
            var person = keys.FirstOrDefault(k => k.Name.Equals(key));
            return person?.GetColoredName();
        }
    }

    [System.Serializable]
    public class PersonName
    {
        public string Name;
        public Color Color;

        public string GetColoredName() => $"<color=#{ColorUtility.ToHtmlStringRGB(Color)}>{Name}</color>";
    }
}