using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Phrase : ScriptableObject
{
    public string phraseId = default;
    [TextArea]
    public string text = default;

    public float popTime = 1.6f;
    [TableList] public List<WordTime> wordTimes = default;

    private void OnValidate() => 
        SeparatePhrase();

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

    public float Duration(TextAppear textAppear)
    {
        if (textAppear is TextAppear.Pop or TextAppear.Fade)
            return popTime;

        return wordTimes.Sum(wt => wt.time);
    }
}