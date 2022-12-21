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

    private bool _showPhrase;

    private void OnEnable()
    {
        if (!_showPhrase) return;

        if (_phraseRoutine != null)
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
        _phrase = phraseSet.Phrase;
        _wordIndex = 0;
        _showPhrase = true;
        
        switch (phraseSet.textAppear)
        {
            case TextAppear.Pop:
                description.text = phraseSet.Phrase.text;
                break;
            case TextAppear.Word:
                if (gameObject is {activeInHierarchy: true, activeSelf: true})
                    _phraseRoutine = StartCoroutine(ShowWords(phraseSet.Phrase, 0));
                break;
            case TextAppear.Letters:
                break;
            case TextAppear.Fade:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator ShowWords(Phrase phrase, int fromWord)
    {
        var text = new StringBuilder();

        if (fromWord == 0)
        {
            yield return new WaitForSeconds(phrase.beforeFirstWord);
        }
        else
        {
            for (var i = 0; i < fromWord; i++)
            {
                var word = GetWord(phrase, i, text);
                text.Append(word);
            }
        }

        for (var i = fromWord; i < phrase.wordTimes.Count; i++)
        {
            _phrase = phrase;
            _wordIndex = i;
            var wordTime = phrase.wordTimes[i].time;
            var word = GetWord(phrase, i, text);

            text.Append(word);

            description.text = text.ToString();

            yield return new WaitForSeconds(wordTime);
        }

        _phrase = null;
        _wordIndex = 0;
        _phraseRoutine = null;
        _showPhrase = false;
    }

    private static string GetWord(Phrase phrase, int i, StringBuilder text)
    {
        var word = phrase.wordTimes[i].word;
        if (phrase.wordTimes[i].wipe)
            text.Clear();

        if (i > 0)
        {
            if (word.Length > 1 && word[0] == '@')
                word = word.Split("@")[1];
            else
                text.Append(" ");
        }

        return word;
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