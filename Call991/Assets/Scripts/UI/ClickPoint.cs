using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ClickPoint : MonoBehaviour
{
    private const float ScaledSize = 1.05f;
    private const float ScaleDuration = 0.35f;
    private const float FadeDuration = 0.3f;

    private RectTransform _rectTransform;
    private Image _image;


    private Sequence _sequence;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        var color = _image.color;
        color.a = 1;
        _image.color = color;

        _rectTransform.localScale = Vector3.one * 0.1f;

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_rectTransform.DOScale(ScaledSize, ScaleDuration).SetEase(Ease.OutBounce));
        _sequence.Append(_image.DOFade(0,FadeDuration)).AppendInterval(0.05f);
        _sequence.Play();
        _sequence.OnComplete(() => gameObject.SetActive(false));
    }
    
    private void OnDisable()
    {
        _sequence?.Kill();
    }
}