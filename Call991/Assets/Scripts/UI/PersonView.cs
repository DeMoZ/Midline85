using System;
using System.Collections;
using System.Text;
using I2.Loc;
using TMPro;
using UI;
using UnityEngine;

public class PersonView : MonoBehaviour
{
    [SerializeField] private ScreenPlace screenPlace = default;
    [SerializeField] private TextMeshProUGUI personName = default;
    [SerializeField] private TextMeshProUGUI description = default;

    public ScreenPlace ScreenPlace => screenPlace;

    private LocalizedString _localize;
    private int _wordIndex;
    private Phrase _phrase;

    private bool _showPhrase;
    private float? _yieldTime;
    private float _wordTime;
    
    private void OnEnable()
    {
        if (!_showPhrase) return;
        
        if(_yieldTime.HasValue)
            StartCoroutine(YieldTime(_wordTime - _yieldTime.Value));
    }
    public void ShowPhrase(UiPhraseData data)
    {
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
    
    private void ShowPhraseText(UiPhraseData data)
    {
        _phrase = data.Phrase;
        _wordIndex = 0;
        _showPhrase = true;
        
        switch (data.PhraseVisualData.TextAppear)
        {
            case TextAppear.Pop:
                description.text = data.Phrase.text;
                break;
            case TextAppear.Word:
                if (gameObject is {activeInHierarchy: true, activeSelf: true})
                {
                    StartCoroutine(ShowWords(data.Phrase, 0));
                }
                else
                {
                    _yieldTime = 0;
                    _wordTime = _phrase.wordTimes[0].time;
                    _wordIndex = -1;
                }
                break;
            case TextAppear.Letters:
                break;
            case TextAppear.Fade:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [Obsolete]
    private void _ShowPhraseText(PhraseSet phraseSet)
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
                {
                    StartCoroutine(ShowWords(phraseSet.Phrase, 0));
                }
                else
                {
                    _yieldTime = 0;
                    _wordTime = _phrase.wordTimes[0].time;
                    _wordIndex = -1;
                }
                break;
            case TextAppear.Letters:
                break;
            case TextAppear.Fade:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator YieldTime(float yieldTime)
    {
        yield return RoutineWaitForSeconds(yieldTime);
        StartCoroutine(ShowWords(_phrase, _wordIndex + 1));
    }

    private IEnumerator RoutineWaitForSeconds(float yieldTime)
    {
        _wordTime = yieldTime;
        _yieldTime = Time.time;
        yield return new WaitForSeconds(yieldTime);
        _yieldTime = null;
        _wordTime = 0;
    }

    private IEnumerator ShowWords(Phrase phrase, int fromWord)
    {
        var text = new StringBuilder();

        if (fromWord == 0)
        {
            yield return RoutineWaitForSeconds(phrase.beforeFirstWord);
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
            _wordTime = phrase.wordTimes[i].time;
            var word = GetWord(phrase, i, text);
            text.Append(word);
            description.text = text.ToString();
            
            yield return RoutineWaitForSeconds(_wordTime);
        }

        _phrase = null;
        _wordIndex = 0;
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

    private void OnDisable()
    {
        if(_yieldTime.HasValue)
            _yieldTime = Time.time - _yieldTime;
    }
}