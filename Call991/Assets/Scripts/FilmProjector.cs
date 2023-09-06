using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
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
    private Vector3 _toHideLocalPos;
    private Vector3 _toShowLocalPos;
    private Vector3 _showLocalPosition;
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
        _currentNodeSlide ??= slide1;
        _nextSlide ??= slide2;
        _sequence?.Kill();

        if (!gameObject.activeSelf)
        {
            slide1.sprite = transparentSlide;
            slide2.sprite = transparentSlide;
            gameObject.SetActive(true);
        }

        if (!_isCalculated) CalculateSliderParameters();

        _changeSlide = _currentNodeSlide;
        _currentNodeSlide = _nextSlide;
        _nextSlide = _changeSlide;

        if (changeCurrentSlide)
        {
            _currentNodeSlide.sprite = sprite;
            return;
        }

        _nextSlide.sprite = sprite;

        _currentNodeSlide.rectTransform.localPosition = _showLocalPosition;
        _nextSlide.rectTransform.localPosition = _toShowLocalPos;

        _sequence = DOTween.Sequence();
        _sequence.SetEase(speedCurve);
        _sequence.Join(_currentNodeSlide.transform.DOLocalMoveY(_toHideLocalPos.y, duration, true));
        _sequence.Join(_nextSlide.transform.DOLocalMoveY(_showLocalPosition.y, duration, true));
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

    [Button("Test Slide")]
    private void TestSlide()
    {
        _currentNodeSlide ??= slide1;
        _nextSlide ??= slide2;
        _sequence?.Kill();

        if (!gameObject.activeSelf)
        {
            slide1.sprite = transparentSlide;
            slide2.sprite = transparentSlide;
            gameObject.SetActive(true);
        }

        if (!_isCalculated) CalculateSliderParameters();

        // if (changeCurrentSlide)
        // {
        //     _currentSlide.sprite = sprite;
        //     return;
        // }
        //
        // _nextSlide.sprite = sprite;
        _changeSlide = _currentNodeSlide;
        _currentNodeSlide = _nextSlide;
        _nextSlide = _changeSlide;

        _currentNodeSlide.rectTransform.localPosition = _showLocalPosition;
        _nextSlide.rectTransform.localPosition = _toShowLocalPos;

        _sequence = DOTween.Sequence();
        _sequence.SetEase(speedCurve);
        _sequence.Join(_currentNodeSlide.transform.DOLocalMoveY(_toHideLocalPos.y, duration, true));
        _sequence.Join(_nextSlide.transform.DOLocalMoveY(_showLocalPosition.y, duration, true));
    }

    private void CalculateSliderParameters()
    {
        _isCalculated = true;

        var maskCorners = new Vector3[4];
        var slideCorners = new Vector3[4];

        slidesParent.GetWorldCorners(maskCorners);
        slide1.rectTransform.GetWorldCorners(slideCorners);

        var topLeftParentCorner = maskCorners[0];
        var topLeftSlideCorner = maskCorners[0];
        var maskHeight = topLeftParentCorner.y - slidesParent.position.y;
        var slideHeight = topLeftSlideCorner.y - slide1.rectTransform.position.y;

        _toHideLocalPos = new Vector3(0, (maskHeight + slideHeight) / 2, 0);
        _toShowLocalPos = -_toHideLocalPos;
    }

    public void AddSlides(List<Sprite> sprites)
    {
        _clickSprites = sprites;
        _currentClickSlide = -1;
    }

    public void OnClick()
    {
        _currentClickSlide++;
        _currentClickSlide = _currentClickSlide >= _clickSprites.Count ? 0 : _currentClickSlide;
        ShowSlide(_clickSprites[_currentClickSlide]);
    }
}