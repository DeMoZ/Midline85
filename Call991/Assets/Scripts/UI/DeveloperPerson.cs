using TMPro;
using UnityEngine;

namespace UI
{
    public class DeveloperPerson : MonoBehaviour
    {
        [SerializeField] private TMP_Text developerPosition;
        [SerializeField] private TMP_Text dash;
        [SerializeField] private TMP_Text developerName;

        public void Set(string devPosition, string devName)
        {
            developerPosition.text = devPosition;
            developerName.text = devName;
            
            dash.gameObject.SetActive(!string.IsNullOrEmpty(devPosition) && !string.IsNullOrEmpty(devName));
        }
    }
}