using System;
using System.Collections;
using System.Text;
using UI;
using UnityEngine;

public class BasePersonView : MonoBehaviour
{
    private int _wordIndex;
    private bool _showPhrase;
    private float? _yieldTime;
    private float _wordTime;
    private Coroutine _phraseRoutine;
    private Coroutine _yieldTimeRoutine;
    private Phrase _phrase;

    private void OnEnable()
    {
        if (!_showPhrase) return;

        if (_yieldTime.HasValue)
        {
            if (_yieldTimeRoutine != null)
            {
                StopCoroutine(_yieldTimeRoutine);
            }

            _yieldTimeRoutine = StartCoroutine(YieldTime(_wordTime - _yieldTime.Value));
        }
    }

    private void OnDisable()
    {
        if (_yieldTime.HasValue)
            _yieldTime = Time.time - _yieldTime;
    }

    protected IEnumerator YieldTime(float yieldTime)
    {
        yield return RoutineWaitForSeconds(yieldTime);
        _phraseRoutine = StartCoroutine(ShowWords(_phrase, _wordIndex + 1));
    }

    protected IEnumerator RoutineWaitForSeconds(float yieldTime)
    {
        _wordTime = yieldTime;
        _yieldTime = Time.time;
        yield return new WaitForSeconds(yieldTime);
        _yieldTime = null;
        _wordTime = 0;
    }
    
    protected void ShowPhraseText(UiPhraseData data)
    {
        _phrase = data.Phrase;
        _wordIndex = 0;
        _showPhrase = true;

        switch (data.PhraseVisualData.TextAppear)
        {
            case TextAppear.Pop:
                SetText(data.Phrase.text);
                break;
            case TextAppear.Word:
                if (gameObject is { activeInHierarchy: true, activeSelf: true })
                {
                    _phraseRoutine = StartCoroutine(ShowWords(data.Phrase, 0));
                }
                else
                {
                    _yieldTime = 0;
                    _wordTime = _phrase.wordTimes[0].timeLine;
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

    protected void ResetRoutines()
    {
        if (_phraseRoutine != null)
            StopCoroutine(_phraseRoutine);

        if (_yieldTimeRoutine != null)
            StopCoroutine(_yieldTimeRoutine);

        _wordIndex = 0;
        _wordTime = 0;
        _showPhrase = false;
        _phrase = null;
        _yieldTime = null;
        _phraseRoutine = null;
        _yieldTimeRoutine = null;
    }

    protected static string GetWord(Phrase phrase, int i, StringBuilder text)
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

    protected IEnumerator ShowWords(Phrase phrase, int fromWord)
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
            SetText(text.ToString());

            yield return RoutineWaitForSeconds(_wordTime);
        }

        ResetRoutines();
    }

    protected virtual void SetText(string text)
    {
        throw new NotImplementedException();
    }
    
    public virtual void HidePhrase()
    {
        throw new NotImplementedException();
    }

    public virtual void Clear()
    {
        throw new NotImplementedException();
    }
    
    public virtual void ShowPhrase(UiPhraseData data)
    {
        throw new NotImplementedException();
    }
}