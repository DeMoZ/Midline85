using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FadePingPong : MonoBehaviour
    {
        [SerializeField] private Image image = default;
        [SerializeField] private float offset = 1;
        private Color _color;
        private void Start()
        {
            _color = image.color;
        }

        private void Update()
        {
            _color.a = Mathf.PingPong(Time.time + offset , 0.8f);
            image.color = _color;
        }
    }
}