using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class PersonView : MonoBehaviour
{
    [SerializeField] private ScreenPlace screenPlace = default;
    [SerializeField] private TextMeshProUGUI personName = default;
    [SerializeField] private TextMeshProUGUI description = default;

    public ScreenPlace ScreenPlace => screenPlace;

    public void ShowPhrase(Phrase phrase)
    {
        description.text = string.Empty;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        personName.text = phrase.GetPersonName();
        description.gameObject.SetActive(true);
        ShowPhraseText(phrase);
    }

    private void ShowPhraseText(Phrase phrase)
    {
        switch (phrase.textAppear)
        {
            case TextAppear.Pop:
                description.text = phrase.text;
                break;
            case TextAppear.Word:
                StartCoroutine(ShowWords(phrase));
                break;
            case TextAppear.Letters:
                break;
            case TextAppear.Fade:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator ShowWords(Phrase phrase)
    {
        var text = new StringBuilder();

        for (var i = 0; i < phrase.wordTime.Count; i++)
        {
            var wordTime = phrase.wordTime[i];
            text.Append(wordTime.word);
            if (i < phrase.wordTime.Count - 1)
                text.Append(" ");
            description.text = text.ToString();

            yield return new WaitForSeconds(wordTime.time);
        }
    }
    
    public void HidePhrase()
    {
        description.gameObject.SetActive(false);
        description.text = string.Empty;
    }

    public void RunText()
    {
        StartCoroutine("RunTextRoutine");
    }


    public void Clear()
    {
        personName.text = string.Empty;
        description.text = string.Empty;
    }
}