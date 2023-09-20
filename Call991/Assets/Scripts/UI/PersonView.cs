using I2.Loc;
using TMPro;
using UI;
using UnityEngine;

public class PersonView : BasePersonView
{
    [SerializeField] private ScreenPlace screenPlace = default;
    [SerializeField] private TextMeshProUGUI personName = default;
    [SerializeField] private TextMeshProUGUI description = default;

    public ScreenPlace ScreenPlace => screenPlace;

    private LocalizedString _localize;


    protected override void SetText(string text)
    {
        description.text = text;
    }

    public override void ShowPhrase(UiPhraseData data)
    {
        ResetRoutines();

        description.text = string.Empty;
        if (data.Phrase != null && data.Phrase.text == AaGraphConstants.None)
        {
            personName.text = string.Empty;
            gameObject.SetActive(false);
            return;
        }

        _localize = data.PersonVisualData.Person;
        personName.text = screenPlace == ScreenPlace.BottomLine ? $"{_localize}:" : _localize;
        description.gameObject.SetActive(true);

        if (data.Phrase != null)
            ShowPhraseText(data);

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public override void HidePhrase()
    {
        description.gameObject.SetActive(false);
        description.text = string.Empty;
    }

    public override void Clear()
    {
        personName.text = string.Empty;
        description.text = string.Empty;
    }
}