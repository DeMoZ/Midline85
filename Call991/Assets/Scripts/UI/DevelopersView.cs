using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class DevelopersView : MonoBehaviour
    {
        [SerializeField] private RectTransform parent;
        [SerializeField] private TMP_Text developerPosition;
        [SerializeField] private TMP_Text developerNamePrefab;

        public void Set(string devPosition, List<string> devNames)
        {
            developerPosition.text = devPosition;
            foreach (var devName in devNames)
            {
                var obj = Instantiate(developerNamePrefab, parent);
                obj.text = devName;
            }
        }
    }
}