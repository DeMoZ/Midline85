using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FilmProjector : MonoBehaviour
{
    [SerializeField] private Image slide1 = default;
    [SerializeField] private Image slide2 = default;
    [SerializeField] private RectTransform slidesParent = default;
    [SerializeField] private Sprite transparentSlide = default;
    [SerializeField] private Button clickBtn = default;

    [SerializeField] private float duration = 1;
    [SerializeField] private AnimationCurve speedCurve = default;

    private Image _currentNodeSlide;
    private Image _nextSlide;
    private Image _changeSlide;

    private List<Sprite> _clickSprites;
    private int _currentClickSlide;

    private bool _isCalculated;
    private float _hideLocalPos;
    private float _appearLocalPos;

    private Sequence _sequence;

    public void Awake()
    {
        OffSlider();
    }

    private void OnEnable()
    {
        clickBtn.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        clickBtn.onClick.RemoveAllListeners();
    }

    public void ShowSlide(Sprite sprite, bool changeCurrentSlide = false)
    {
        if (!gameObject.activeSelf)
        {
            slide1.sprite = transparentSlide;
            slide2.sprite = transparentSlide;
            gameObject.SetActive(true);
        }

        _currentNodeSlide ??= slide1;
        _nextSlide ??= slide2;
        _sequence?.Kill();

        if (!_isCalculated)
            CalculateSliderParameters();

        _changeSlide = _currentNodeSlide;
        _currentNodeSlide = _nextSlide;
        _nextSlide = _changeSlide;

        if (changeCurrentSlide)
        {
            _currentNodeSlide.sprite = sprite;
            return;
        }

        _nextSlide.sprite = sprite;

        _currentNodeSlide.rectTransform.localPosition = Vector3.zero;
        _nextSlide.rectTransform.localPosition = Vector3.up * _appearLocalPos;

        _sequence = DOTween.Sequence();
        _sequence.SetEase(speedCurve);
        _sequence.Append(_currentNodeSlide.transform.DOLocalMoveY(_hideLocalPos, duration, true));
        _sequence.Join(_nextSlide.transform.DOLocalMoveY(0, duration, true));
    }

    public void HideSlide()
    {
        ShowSlide(transparentSlide);
    }

    public void OffSlider()
    {
        slide1.sprite = transparentSlide;
        slide2.sprite = transparentSlide;
        gameObject.SetActive(false);
    }

    private void CalculateSliderParameters()
    {
        _isCalculated = true;
        var sizeDeltaY = slide1.rectTransform.sizeDelta.y;
        _hideLocalPos = sizeDeltaY;
        _appearLocalPos = -sizeDeltaY;
    }

    public void AddSlides(List<Sprite> sprites)
    {
        _clickSprites = sprites;
        _currentClickSlide = -1;
    }

    private void OnClick()
    {
        if (_clickSprites == null || _clickSprites.Count < 1) return;

        _currentClickSlide++;
        _currentClickSlide = _currentClickSlide >= _clickSprites.Count ? 0 : _currentClickSlide;
        ShowSlide(_clickSprites[_currentClickSlide]);
    }
}