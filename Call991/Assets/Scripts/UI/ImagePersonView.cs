using AaDialogueGraph;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class ImagePersonView : BasePersonView
{
    [SerializeField] private PersonImageScreenPlace screenPlace = default;
    [SerializeField] private Image person = default;
    [SerializeField] private CanvasGroup canvasGroup = default;
    [SerializeField] private AnimationCurve animateCurve = default;
    [SerializeField] private ImagePersonViewConfig config = default;

    private ImagePersonVisualData _personVisualData;
    private Sequence _sequence;

    public PersonImageScreenPlace ScreenPlace => screenPlace;

    public void ShowPhrase(UiImagePhraseData data)
    {
        ResetRoutines();
        ResetSequence();

        if (data == null)
        {
            person.gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(!string.IsNullOrEmpty(data.PersonVisualData.Sprite));

        person.sprite = data.Sprite;

        _personVisualData = data.PersonVisualData;
        var tf = person.rectTransform;

        if (_personVisualData.ShowOnStart)
        {
            var position = tf.localPosition;
            tf.localPosition = position + Vector3.right * ((int)screenPlace > 3 ? 1 : -1) * config.MoveValue;
            _sequence.Append(tf.DOLocalMoveX(position.x, config.FadeTime).SetEase(animateCurve));

            person.color = config.NormalColor;
            canvasGroup.alpha = 0;
            _sequence.Join(canvasGroup.DOFade(1, config.FadeTime));
        }
        else if (_personVisualData.FocusOnStart)
        {
            tf.localScale = Vector3.one * config.ScaleValue;
            _sequence.Append(person.transform.DOScale(Vector3.one, config.FadeTime));
            _sequence.Join(person.DOColor(config.NormalColor, config.FadeTime));
        }
    }

    private void ResetSequence()
    {
        _sequence?.Kill();
        _sequence = DOTween.Sequence();
    }

    public override void HidePhrase()
    {
        ResetSequence();

        if (_personVisualData.UnfocusOnEnd)
        {
            _sequence.Append(person.transform.DOScale(Vector3.one * config.ScaleValue, config.FadeTime));
            _sequence.Join(person.DOColor(config.ShadeColor, config.FadeTime));
        }

        if (_personVisualData.HideOnEnd)
        {
            var tf = person.transform;
            _sequence.Append(canvasGroup.DOFade(0, config.FadeTime));
            var position = tf.localPosition - Vector3.right * ((int)screenPlace > 3 ? -1 : 1) * config.MoveValue;
            _sequence.Join(tf.DOLocalMoveX(position.x, config.FadeTime)
                .OnComplete(() => gameObject.SetActive(false)));
        }
    }

    public override void Clear()
    {
    }
}