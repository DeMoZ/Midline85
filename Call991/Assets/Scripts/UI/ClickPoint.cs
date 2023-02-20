using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class ClickPoint : MonoBehaviour
{
    [SerializeField] private ScaleSet scaleSet;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float fadeDelay = 0.05f;

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

        _rectTransform.localScale = Vector3.one;

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_rectTransform.DOScale(scaleSet.Size, scaleSet.Duration).
            SetEase(scaleSet.Ease));
        _sequence.Append(_image.DOFade(0,fadeDuration)).AppendInterval(fadeDelay);
        _sequence.Play();
        _sequence.OnComplete(() => gameObject.SetActive(false));
    }
    
    private void OnDisable()
    {
        _sequence?.Kill();
    }
}