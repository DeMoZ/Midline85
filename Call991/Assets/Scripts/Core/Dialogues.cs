using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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

            if (phrase.textAppear == TextAppear.Pop)
                phrase.appearDuration = 0;

            phrase.appearDuration = Mathf.Clamp(phrase.appearDuration, 0, phrase.duration);
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
    public float duration = 2;
    [VerticalGroup("Dialog")]
    public TextAppear textAppear;

    [Tooltip("Dialog will appear during that time (slow or fast)")]
    [VerticalGroup("Dialog")]
    [ShowIf("ShowAppearDuration")] public float appearDuration;

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
    private bool ShowAppearDuration() =>
        textAppear != TextAppear.Pop;

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