using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Phrase : ScriptableObject
{
    public string phraseId = default;

    [HorizontalGroup("SoundName")] public bool overrideSoundName;

    [ShowIf("overrideSoundName")] [HorizontalGroup("SoundName")] [HideLabel]
    public string soundFileName;

    [TextArea] public string text = default;

    [TableList] public List<WordTime> wordTimes = default;

    [Tooltip("Time await BEFORE FIRST word appear")] [ReadOnly]
    public float beforeFirstWord = 0.01f;

    [Tooltip("Time await AFTER LAST word appear")]
    public float afterLastWord = 1.6f;

    [Tooltip("Full time text appear")] [ReadOnly]
    public float totalTime = 1.6f;

    public string GetOverridenPhraseId() =>
        overrideSoundName ? soundFileName : phraseId;

    private void OnValidate()
    {
        // ResetWordTimes(); // for fix 
        SeparatePhrase();
        AdjustTimeLine();
        SetTime();
        SetPopTime();
    }

    [Button]
    private void ResetWordTimes()
    {
        wordTimes.Clear();
        SeparatePhrase();
    }

    public void SeparatePhrase()
    {
        if (wordTimes == null || wordTimes.Count == 0)
        {
            var words = text.Split(" ");
            wordTimes ??= new List<WordTime>();
            beforeFirstWord = 0.1f;
            afterLastWord = 1.6f;
            var prevTime = beforeFirstWord;
            for (var w = 0; w < words.Length; w++)
            {
                var word = words[w];
                wordTimes.Add(new WordTime
                {
                    word = word,
                    timeLine = prevTime
                });

                prevTime += 0.4f;
            }

            wordTimes[^1].time = afterLastWord;
        }
    }

    private void AdjustTimeLine()
    {
        if (wordTimes is {Count: > 0})
            wordTimes[0].timeLine = Mathf.Clamp(wordTimes[0].timeLine, 0, float.MaxValue);

        if (wordTimes.Count <= 1) return;

        for (var i = 1; i < wordTimes.Count; i++)
        {
            if (wordTimes[i].timeLine < wordTimes[i - 1].timeLine)
                wordTimes[i].timeLine = wordTimes[i - 1].timeLine;
        }
    }

    private void SetTime()
    {
        beforeFirstWord = wordTimes is {Count: > 0} ? wordTimes[0].timeLine : beforeFirstWord = 0;
        afterLastWord = Mathf.Clamp(afterLastWord, 0, float.MaxValue);

        if (wordTimes.Count <= 1) return;

        for (var i = 1; i < wordTimes.Count; i++)
        {
            wordTimes[i - 1].time = wordTimes[i].timeLine - wordTimes[i - 1].timeLine;
        }

        wordTimes[^1].time = afterLastWord;
    }

    private void SetPopTime()
    {
        totalTime = afterLastWord;
        if (wordTimes.Count > 0)
            totalTime += wordTimes[^1].timeLine;
    }

    public float Duration(TextAppear textAppear)
    {
        if (textAppear is TextAppear.Pop or TextAppear.Fade)
            return totalTime + beforeFirstWord;

        return wordTimes.Sum(wt => wt.time) + beforeFirstWord;
    }
}