using UnityEngine;
using TMPro;

namespace UI
{
    public class DisableInTime : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI[] _text;
        [SerializeField] private float speed = 0.005f;
        int actualText;
        float alphaStart = 0f;
        float alphaStop = 1f;
        float alpha;

        private void Start()
        {
            alpha = alphaStart;
            for (int i = 0; i < _text.Length; i++)
            {
                _text[i].GetComponent<CanvasRenderer>().SetAlpha(alpha);
            }

            //Invoke(nameof(DisableIn), time);
        }

        private void Update()
        {
            if (actualText < _text.Length)
            {
                if (alpha < alphaStop)
                {

                    alpha = alpha + speed;
                    _text[actualText].GetComponent<CanvasRenderer>().SetAlpha(alpha);
                }
                else
                {
                    actualText++;
                    if (actualText < _text.Length)
                    {
                        alpha = alphaStart;
                    }
                }
            }
            /*else
            {
                if (alpha > alphaStart)
                {
                    alpha = alpha - speed;
                    for (int i = 0; i < _text.Length; i++)
                    {
                        _text[i].GetComponent<CanvasRenderer>().SetAlpha(alpha);
                    }

                }
            }*/
        }

        private void DisableIn()
        {
            gameObject.SetActive(false);
        }
    }
}