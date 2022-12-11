using System.Collections;
using DG.Tweening;
using UnityEngine;
using TMPro;

namespace UI
{
    public class UiLoadingTitle : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI[] texts = default;
        
        private float _fadeTime = 1;

        public void SetCtx(float fadeTime)
        {
            _fadeTime = fadeTime;
        }
        
        private void Start()
        {
            foreach (var text in texts)
                text.DOFade(0, 0);

            if (texts.Length > 0)
                StartCoroutine(ApplyAlpha(0));
        } 

        private IEnumerator ApplyAlpha(int index)
        {
            texts[index].DOFade(1, _fadeTime);
            yield return new WaitForSeconds(_fadeTime);
            index++;
            
            if(index < texts.Length )
                StartCoroutine(ApplyAlpha(index));
        }
    }
}