using UnityEngine;

namespace UI
{
    public class DisableInTime : MonoBehaviour
    {
        [SerializeField] private float time = 4f;
        private void Start()
        {
            Invoke(nameof(DisableIn),time);
        }

        private void DisableIn()
        {
            gameObject.SetActive(false);
        }
    }
}