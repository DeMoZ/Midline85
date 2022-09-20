using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Phrase : ScriptableObject
{
    public string phraseId = default;
    [TextArea] public string text = default;

    [Tooltip("Time for instant text appear")]
    public float popTime = 1.6f;

    //public bool setTimeLine = false;

    //[ShowIf("setTime")] 
    [Tooltip("Time await before FIRST word appear")]
    public float firstWordOffset = 0.01f;

    [TableList] public List<WordTime> wordTimes = default;

    //[ShowIf("setTimeLine")][Tooltip("Time await after LAST word appear")] [TableList]
    //public float lastWordOffset = 1.6f;

    //private bool setTime => !setTimeLine;

    private void OnValidate()
    {
        SeparatePhrase();

        //if (setTimeLine)
            SetTimeLine();
        //else
        //    SetTime();
    }

    public void SeparatePhrase()
    {
        if (wordTimes == null || wordTimes.Count == 0)
        {
            var words = text.Split(" ");
            wordTimes ??= new List<WordTime>();

            for (var w = 0; w < words.Length; w++)
            {
                var word = words[w];
                wordTimes.Add(new WordTime {word = word});

                if (w == words.Length - 1)
                    wordTimes[^1].time *= 4;
            }
        }
    }

    private void SetTimeLine()
    {
        var prevTime = firstWordOffset;
        foreach (var word in wordTimes)
        {
            word.timeLine = prevTime;
            prevTime += word.time;
        }
    }

    // private void SetTime()
    // {
    // }

    public float Duration(TextAppear textAppear)
    {
        if (textAppear is TextAppear.Pop or TextAppear.Fade)
            return popTime + firstWordOffset;

        return wordTimes.Sum(wt => wt.time) + firstWordOffset;
    }
}