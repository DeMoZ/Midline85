using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class FilmProjector : MonoBehaviour
{
    [SerializeField] private Image slide1 = default;
    [SerializeField] private Image slide2 = default;
    [SerializeField] private RectTransform slidesParent = default;
    [SerializeField] private Sprite _transparentSlide = default;
    
    [SerializeField] private float duration = 1;
    [SerializeField] private AnimationCurve speedCurve = default;

    private Image _currentSlide;
    private Image _nextSlide;
    private Image _changeSlide;

    private bool _isCalculated;
    private Vector3 _toHideLocalPos;
    private Vector3 _toShowLocalPos;
    private Vector3 _showLocalPosition;
    private Sequence _sequence;
    
    public void Awake()
    {
        OffSlider();
    }
    
    public void ShowSlide(Sprite sprite, bool changeCurrentSlide = false)
    {
        _currentSlide ??= slide1;
        _nextSlide ??= slide2;
        _sequence?.Kill();

        if (!gameObject.activeSelf)
        {
            slide1.sprite = _transparentSlide;
            slide2.sprite = _transparentSlide; 
            gameObject.SetActive(true);
        }

        if (!_isCalculated) CalculateSliderParameters();
        
        _changeSlide = _currentSlide;
        _currentSlide = _nextSlide;
        _nextSlide = _changeSlide;

        if (changeCurrentSlide)
        {
            _currentSlide.sprite = sprite;
            return;
        }
        _nextSlide.sprite = sprite;
        
        _currentSlide.rectTransform.localPosition = _showLocalPosition;
        _nextSlide.rectTransform.localPosition = _toShowLocalPos;

        _sequence = DOTween.Sequence();
        _sequence.SetEase(speedCurve);
        _sequence.Join(_currentSlide.transform.DOLocalMoveY(_toHideLocalPos.y, duration, true));
        _sequence.Join(_nextSlide.transform.DOLocalMoveY(_showLocalPosition.y, duration, true));
    }

    public void HideSlide()
    {
        ShowSlide(_transparentSlide);
    }
    
    public void OffSlider()
    {
        slide1.sprite = _transparentSlide;
        slide2.sprite = _transparentSlide; 
        gameObject.SetActive(false);
    }

    [Button("Test Slide")]
    private void TestSlide()
    {
        _currentSlide ??= slide1;
        _nextSlide ??= slide2;
        _sequence?.Kill();

        if (!gameObject.activeSelf)
        {
            slide1.sprite = _transparentSlide;
            slide2.sprite = _transparentSlide; 
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
        _changeSlide = _currentSlide;
        _currentSlide = _nextSlide;
        _nextSlide = _changeSlide;

        _currentSlide.rectTransform.localPosition = _showLocalPosition;
        _nextSlide.rectTransform.localPosition = _toShowLocalPos;

        _sequence = DOTween.Sequence();
        _sequence.SetEase(speedCurve);
        _sequence.Join(_currentSlide.transform.DOLocalMoveY(_toHideLocalPos.y, duration, true));
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
}