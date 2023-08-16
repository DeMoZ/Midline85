using I2.Loc;
using TMPro;
using UI;
using UnityEngine;

public class TextPersonView : BasePersonView
{
    [SerializeField] private TextMeshProUGUI description = default;
    
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

        _localize = data.PersonVisualData.Person;
        description.gameObject.SetActive(data.Phrase != null && data.Phrase.text != AaGraphConstants.None);

        if (data.Phrase == null)
            description.text = string.Empty;
        else
            ShowPhraseText(data);
    }

    public override void HidePhrase()
    {
        description.gameObject.SetActive(false);
        description.text = string.Empty;
    }

    public override void Clear()
    {
        description.text = string.Empty;
    }
}