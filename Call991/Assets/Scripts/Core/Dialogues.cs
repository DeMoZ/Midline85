using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu]
public class Dialogues : ScriptableObject
{
    [TableList(NumberOfItemsPerPage = 10,ShowIndexLabels = true)]
    public List<Phrase> phrases;

    private void OnValidate()
    {
        for (var i = 0; i < phrases.Count; i++)
        {
            var phrase = phrases[i];
            
            if (phrase.nextIs == NextIs.Phrase)
            {
                phrase.choices = new List<Choice>();
                phrase.overrideChoicesDuration = false;
            }
            else
            {
                phrase.nextId = null;
            }

            if (phrase.textAppear is TextAppear.Pop or TextAppear.Fade)
            {
                phrase.wordTime = null;
            }
            else
            {
                // phrase.wordTime.Clear();
                if (phrase.wordTime == null || phrase.wordTime.Count == 0)
                {
                    var words = phrase.text.Split(" ");
                    phrase.wordTime ??= new List<WordTime>();

                    for (var w = 0; w < words.Length; w++)
                    {
                        var word = words[w];
                        phrase.wordTime.Add(new WordTime {word = word});

                        if (w == words.Length - 1)
                            phrase.wordTime[^1].time *= 4;
                    }
                }
            }
        }
    }
}

[Serializable]
public class Phrase
{
    [VerticalGroup("phraseId")][TableColumnWidth(90, false)][HideLabel]
    public string phraseId;
    
    [VerticalGroup("Person")][TableColumnWidth(150, false)]
    public Person person;
    [VerticalGroup("Person")][Tooltip("Place on screen")]
    public ScreenPlace screenPlace = ScreenPlace.MiddleLeft;
    [Tooltip("Person will hide on end phrase")]
    [VerticalGroup("Person")]
    public bool hidePersonOnEnd = false;
    
    [VerticalGroup("Dialog")][TableColumnWidth(220, false)]
    public string text;
    [VerticalGroup("Dialog")]
    public TextAppear textAppear;

    [Tooltip("Dialog will appear during that time (slow or fast)")]
    [VerticalGroup("Dialog")][TableList(ShowIndexLabels = true)][OdinSerialize]
    [ShowIf("ShowIfNotPop")] public List<WordTime> wordTime;
    [VerticalGroup("Dialog")]
    [ShowIf("ShowIfPopOrFade")][SerializeField] private float duration = 2;
    [Tooltip("Dialog will hide on end phrase")]
    [VerticalGroup("Dialog")]
    public bool hidePhraseOnEnd= true; // next is choices of phrase
    
    [VerticalGroup("Next")] [TableColumnWidth(260, false)]
    public NextIs nextIs;

    [VerticalGroup("Next")]
    [ShowIf("nextIs", NextIs.Phrase)] public string nextId;
    // or
    [VerticalGroup("Next")]
    [ShowIf("nextIs", NextIs.Choices)] public List<Choice> choices;

    [VerticalGroup("Next")]
    [ShowIf("nextIs", NextIs.Choices)] public bool overrideChoicesDuration;
    [VerticalGroup("Next")]
    [ShowIf("overrideChoicesDuration", true)] public float choicesDuration;
    
    
    [VerticalGroup("Event")] [TableColumnWidth(200, false)]
    public bool addEvent;
    [VerticalGroup("Event")] [ShowIf("addEvent")] public List<DialogueEvent> dialogueEvents;

    public float Duration
    {
        get
        {
            if (textAppear is TextAppear.Pop or TextAppear.Fade)
                return duration;
            
            return wordTime.Sum(wt => wt.time);
        }
    }

    private bool ShowIfNotPop() =>
        textAppear != TextAppear.Pop;
    
    private bool ShowIfPopOrFade() =>
        textAppear is TextAppear.Pop or TextAppear.Fade ;

    public string GetPersonName() => 
        person.ToString();
}

[Serializable]
public class DialogueEvent
{
    public string eventId;
    public float delay;
}

[Serializable]
public class Choice
{
    public string choiceId;
    public string text;
    public string nextPhraseId;
    
    [Tooltip("If some previous choices")]
    public bool ifSelected;
    [ShowIf("ifSelected")]
    public List<string> requiredChoices;
}

[Serializable]
public class WordTime
{
    [TableColumnWidth(100)]
    public string word;
    [TableColumnWidth(50)]
    public float time = 0.4f;
}