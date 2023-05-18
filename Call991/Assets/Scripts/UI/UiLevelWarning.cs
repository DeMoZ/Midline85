using System.Collections;
using DG.Tweening;
using UnityEngine;
using TMPro;

namespace UI
{
    public class UiLevelWarning : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textsPrefab = default;
        [SerializeField] private RectTransform textsParent = default;
        
        private TextMeshProUGUI[] _texts;

        private float _delayTime;
        private float _fadeTime;
        
        public void Set(string[] keys, float delayTime, float fadeTime)
        {
            _delayTime = delayTime;
            _fadeTime = fadeTime;
            _texts = new TextMeshProUGUI[keys.Length];

            foreach (Transform child in textsParent)
            {
                Destroy(child.gameObject);
            }
            
            for (var i = 0; i < _texts.Length; i++)
            {
                var go = Instantiate(textsPrefab, textsParent);
                go.text = keys[i];
                _texts[i] = go;
            }
        }
        
        private void Start()
        {
            foreach (var text in _texts)
                text.DOFade(0, 0);

            if (_texts.Length > 0)
                StartCoroutine(ApplyAlpha());
        }

        private IEnumerator ApplyAlpha()
        {
            var waitTime = new WaitForSeconds(_fadeTime + _delayTime);

            for (var i = 0; i < _texts.Length; i++)
            {
                _texts[i].DOFade(1, _fadeTime);
                yield return waitTime;
            }
        }
    }
}