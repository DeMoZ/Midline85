using UI;
using UnityEngine;
using UnityEngine.UI;

public class ImagePersonView : BasePersonView
{
    [SerializeField] private PersonImageScreenPlace screenPlace = default;
    [SerializeField] private Image person = default;
    [SerializeField] private Image mask = default;
    [SerializeField] private Image shade = default;

    public PersonImageScreenPlace ScreenPlace => screenPlace;
    
    public override void ShowPhrase(UiPhraseData data)
    {
        ResetRoutines();
        
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        
        //description.gameObject.SetActive(true);

        if (data.Phrase == null)
        {
            //description.text = data.Description;
        }
        else
        {
            ShowPhraseText(data);
        }
    }

    public override void HidePhrase()
    {
        // description.gameObject.SetActive(false);
        // description.text = string.Empty;
    }

    public override void Clear()
    {
        
    }
}