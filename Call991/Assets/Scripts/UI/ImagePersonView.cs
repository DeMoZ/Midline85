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
    [SerializeField] private ImagePersonViewConfig config = default;
    
    private ImagePersonVisualData _personVisualData;

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
            tf.localPosition = position + Vector3.right * ((int)screenPlace > 3 ? 1 : -1) * config.MoveValue;
            tf.DOLocalMoveX(position.x, config.FadeTime).SetEase(animateCurve);

            shadeCanvasGroup.alpha = 0;
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, config.FadeTime);
        }
        else if (_personVisualData.FocusOnStart)
        {
            tf.localScale = Vector3.one * config.ScaleValue;
            person.transform.DOScale(Vector3.one, config.FadeTime);
            shadeCanvasGroup.DOFade(0, config.FadeTime);
        }
    }

    public override void HidePhrase()
    {
        if (_personVisualData.UnfocusOnEnd)
        {
            person.transform.DOScale(Vector3.one * config.ScaleValue, config.FadeTime);
            shadeCanvasGroup.DOFade(config.FadeValue, config.FadeTime);
        }

        if (_personVisualData.HideOnEnd)
        {
            var tf = person.transform;
            canvasGroup.DOFade(0, config.FadeTime);
            var position = tf.localPosition - Vector3.right * ((int)screenPlace > 3 ? -1 : 1) * config.MoveValue;
            tf.DOLocalMoveX(position.x, config.FadeTime)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }

    public override void Clear()
    {
    }
}