using System;
using System.Collections;
using System.Text;
using I2.Loc;
using TMPro;
using UnityEngine;

public class PersonView : MonoBehaviour
{
    [SerializeField] private ScreenPlace screenPlace = default;
    [SerializeField] private TextMeshProUGUI personName = default;
    [SerializeField] private TextMeshProUGUI description = default;

    public ScreenPlace ScreenPlace => screenPlace;

    private LocalizedString _localize;
    private Coroutine _phraseRoutine;
    private int _wordIndex;
    private Phrase _phrase;

    private void OnEnable()
    {
        if (_phraseRoutine == null) return;

        StopCoroutine(_phraseRoutine);
        _phraseRoutine = StartCoroutine(ShowWords(_phrase, _wordIndex));
    }

    public void ShowPhrase(PhraseSet phrase)
    {
        description.text = string.Empty;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        _localize = phrase.GetPersonName();
        personName.text = _localize;
        description.gameObject.SetActive(true);
        ShowPhraseText(phrase);
    }

    private void ShowPhraseText(PhraseSet phraseSet)
    {
        switch (phraseSet.textAppear)
        {
            case TextAppear.Pop:
                description.text = phraseSet.Phrase.text;
                break;
            case TextAppear.Word:
                _phraseRoutine = StartCoroutine(ShowWords(phraseSet.Phrase));
                break;
            case TextAppear.Letters:
                break;
            case TextAppear.Fade:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator ShowWords(Phrase phrase, int fromWord = 0)
    {
        var text = new StringBuilder();

        if (fromWord == 0)
            yield return new WaitForSeconds(phrase.beforeFirstWord);

        for (var i = fromWord; i < phrase.wordTimes.Count; i++)
        {
            _phrase = phrase;
            _wordIndex = i;
            var wordTime = phrase.wordTimes[i];
            var word = wordTime.word;
            if (phrase.wordTimes[i].wipe)
                text.Clear();

            if (i > 0)
            {
                if (word.Length > 1 && word[0] == '@')
                {
                    word = word.Split("@")[1];
                }
                else
                {
                    text.Append(" ");
                }
            }

            text.Append(word);

            description.text = text.ToString();

            yield return new WaitForSeconds(wordTime.time);
        }

        _phrase = null;
        _wordIndex = 0;
        _phraseRoutine = null;
    }

    public void HidePhrase()
    {
        description.gameObject.SetActive(false);
        description.text = string.Empty;
    }

    public void Clear()
    {
        personName.text = string.Empty;
        description.text = string.Empty;
    }
}