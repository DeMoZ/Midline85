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

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        _localize = data.PersonVisualData.Person.ToString();
        personName.text = _localize;
        description.gameObject.SetActive(true);

        if (data.Phrase == null)
        {
            description.text = data.Description;
        }
        else
        {
            ShowPhraseText(data);
        }
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