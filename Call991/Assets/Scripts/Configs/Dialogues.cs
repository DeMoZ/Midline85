using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Dialogues : ScriptableObject
{
    [TableList(NumberOfItemsPerPage = 10, ShowIndexLabels = true)]
    public List<PhraseSet> phrases;

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
        }
    }
}

[Serializable]
public class PhraseSet
{
    [VerticalGroup("phraseId")] [TableColumnWidth(90, false)] [HideLabel]
    public string phraseId;
    
    [VerticalGroup("Person")] [TableColumnWidth(150, false)]
    public string person;

    [VerticalGroup("Person")] [Tooltip("Place on screen")]
    public ScreenPlace screenPlace = ScreenPlace.MiddleLeft;

    [Tooltip("Person will hide on end phrase")] [VerticalGroup("Person")]
    public bool hidePersonOnEnd = false;

    [VerticalGroup("Dialog")] [TableColumnWidth(220, false)]
    public TextAppear textAppear;

    [Tooltip("Dialog will hide on end phrase")] [VerticalGroup("Dialog")]
    public bool hidePhraseOnEnd = true; // next is choices of phrase

    [VerticalGroup("Next")] [TableColumnWidth(260, false)]
    public NextIs nextIs;

    [VerticalGroup("Next")] [ShowIf("nextIs", NextIs.Phrase)]
    public string nextId;

    // or
    [VerticalGroup("Next")] [ShowIf("nextIs", NextIs.Choices)]
    public List<Choice> choices;

    [VerticalGroup("Next")] [ShowIf("nextIs", NextIs.Choices)]
    public bool overrideChoicesDuration;

    [VerticalGroup("Next")] [ShowIf("overrideChoicesDuration", true)]
    public float choicesDuration;


    [VerticalGroup("Event")] [TableColumnWidth(200, false)]
    public bool addEvent;

    [VerticalGroup("Event")] [ShowIf("addEvent")]
    public List<PhraseEvent> phraseEvents;

    private bool ShowIfNotPop() =>
        textAppear != TextAppear.Pop;

    private bool ShowIfPopOrFade() =>
        textAppear is TextAppear.Pop or TextAppear.Fade;
    
    public Phrase Phrase { get; set; }

    public string GetPersonName() =>
        person.ToString();

    public override string ToString() => 
        $"phraseId: {phraseId}; person: {person}; screenPlace: {screenPlace}; nextIs: {nextIs}; nextId: {nextId};";
}

[Serializable]
public class Choice
{
    public string choiceId;
    public string nextPhraseId;
    
    [Tooltip("If some previous choices")]
    public bool ifSelected;
    [ShowIf("ifSelected")]
    public List<string> requiredChoices;
}