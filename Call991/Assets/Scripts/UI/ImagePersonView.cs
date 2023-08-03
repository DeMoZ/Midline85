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
    [SerializeField] private Image shade = default;

    private ImagePersonVisualData _personVisualData;
    private float _fadeTime = 0.7f;
    private float _fadeValue = 0.6f;
    private float _scaleValue = 1.15f;
    private float _moveValue = 150f;

    public PersonImageScreenPlace ScreenPlace => screenPlace;

    public void ShowPhrase(UiImagePhraseData data)
    {
        ResetRoutines();

        if (data == null)
        {
            person.gameObject.SetActive(false);
            return;
        }

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        person.sprite = data.Sprite;
        mask.sprite = data.Sprite;

        _personVisualData = data.PersonVisualData;

        if (_personVisualData.ShowOnStart)
        {
            var tf = person.transform;
            var position = tf.localPosition;
            tf.localPosition = position + Vector3.right * ((int)screenPlace > 3 ? 1 : -1) * _moveValue;
            tf.DOLocalMoveX(position.x, _fadeTime);

            // todo fade person and all the group from transparent to 1;
            person.DOFade(0, 0);
            person.DOFade(1, _fadeTime);
        }
        
        // todo focus shouldnt be done with fade (no shade fade), if image is appearing
        if (_personVisualData.FocusOnStart)
            Focus(0, _scaleValue);
    }

    public override void HidePhrase()
    {
        if (_personVisualData.UnfocusOnEnd)
            Focus(_fadeValue, 1);

        if (_personVisualData.HideOnEnd)
        {
            var tf = person.transform;
            var position = tf.localPosition - Vector3.right * ((int)screenPlace > 3 ? -1 : 1) * _moveValue;
            tf.DOLocalMoveX(position.x, _fadeTime)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }

    private void Focus(float shadeValue, float scaleValue)
    {
        shade.DOFade(shadeValue, _fadeTime);
        person.transform.DOScale(scaleValue, _fadeTime);
    }

    public override void Clear()
    {
    }
}