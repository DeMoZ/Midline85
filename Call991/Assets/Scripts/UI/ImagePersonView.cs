using AaDialogueGraph;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class ImagePersonView : BasePersonView
{
    [SerializeField] private PersonImageScreenPlace screenPlace = default;
    [SerializeField] private Image person = default;
    [SerializeField] private Image mask = default;

    [SerializeField] private CanvasGroup canvasGroup = default;
    [SerializeField] private CanvasGroup shadeCanvasGroup = default;

    [SerializeField] private AnimationCurve animateCurve = default;

    private ImagePersonVisualData _personVisualData;
    private float _fadeTime = 0.7f;
    private float _fadeValue = 0.45f;
    private float _scaleValue = 0.85f;
    private float _moveValue = 250f;

    public PersonImageScreenPlace ScreenPlace => screenPlace;

    public void ShowPhrase(UiImagePhraseData data)
    {
        ResetRoutines();

        if (data == null)
        {
            person.gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(!string.IsNullOrEmpty(data.PersonVisualData.Sprite));

        person.sprite = data.Sprite;
        mask.sprite = data.Sprite;

        _personVisualData = data.PersonVisualData;
        var tf = person.rectTransform;

        if (_personVisualData.ShowOnStart)
        {
            var position = tf.localPosition;
            tf.localPosition = position + Vector3.right * ((int)screenPlace > 3 ? 1 : -1) * _moveValue;
            tf.DOLocalMoveX(position.x, _fadeTime).SetEase(animateCurve);

            shadeCanvasGroup.alpha = 0;
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, _fadeTime);
        }
        else if (_personVisualData.FocusOnStart)
        {
            tf.localScale = Vector3.one * _scaleValue;
            person.transform.DOScale(Vector3.one, _fadeTime);
            shadeCanvasGroup.DOFade(0, _fadeTime);
        }
    }

    public override void HidePhrase()
    {
        if (_personVisualData.UnfocusOnEnd)
        {
            person.transform.DOScale(Vector3.one * _scaleValue, _fadeTime);
            shadeCanvasGroup.DOFade(_fadeValue, _fadeTime);
        }

        if (_personVisualData.HideOnEnd)
        {
            var tf = person.transform;
            canvasGroup.DOFade(0, _fadeTime);
            var position = tf.localPosition - Vector3.right * ((int)screenPlace > 3 ? -1 : 1) * _moveValue;
            tf.DOLocalMoveX(position.x, _fadeTime)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }

    public override void Clear()
    {
    }
}