using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum ScreenPlace
{
    None,
    TopLeft,
    MiddleLeft,
    BottomLeft,
    TopRight,
    MiddleRight,
    BottomRight
}

public enum Person
{
    None,
    Emma,
    Andreas,
}

public enum TextAppear
{
    Pop,
    Letters,
    Word,
    Fade,
}

public enum NextIs
{
    Phrase,
    Choices,
}

[CreateAssetMenu]
public class Dialogues : ScriptableObject
{
    [TableList]//[FoldoutGroup("1")]
    public List<Phrase> phrases;

    private void OnValidate()
    {
        foreach (var phrase in phrases)
        {
            if (phrase.nextIs == NextIs.Phrase)
                phrase.choices = new List<Choice>();
            else
                phrase.nextId = null;

            phrase.appearDelay =
                phrase.textAppear == TextAppear.Pop ? phrase.appearDelay = 0 : phrase.appearDelay = 0.1f;
        }
    }
}

[Serializable]
public class Phrase
{
    //[FoldoutGroup("Phrase")]
    [VerticalGroup("phraseId")]
    public string phraseId;
    
    [VerticalGroup("Person")]
    public Person person;
    [VerticalGroup("Person")]
    public ScreenPlace screenPlace = ScreenPlace.MiddleLeft;
    
    [VerticalGroup("Dialog")]
    public string description;
    [VerticalGroup("Dialog")]
    public float durationAfterAppear = 2;
    [VerticalGroup("Dialog")]
    public TextAppear textAppear;

    [VerticalGroup("Dialog")]
    [ShowIf("ShowAppearDelay")] public float appearDelay;
    
    [VerticalGroup("Dialog")]
    public bool hideOnNext = true; // next is choices of phrase
    
    [VerticalGroup("Next")]
    public NextIs nextIs;

    [VerticalGroup("Next")]
    [ShowIf("nextIs", NextIs.Phrase)] public string nextId;
    // or
    [VerticalGroup("Next")]
    [ShowIf("nextIs", NextIs.Choices)] public List<Choice> choices;

    [VerticalGroup("Event")]
    public bool addEvent;
    [VerticalGroup("Event")] [ShowIf("addEvent")] public List<DialogueEvent> dialogueEvents;
    private bool ShowAppearDelay() =>
        textAppear != TextAppear.Pop;
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
    public string description;
    public string nextPhraseId;
    
    [Tooltip("If some previous choices")]
    public bool ifSelected;
    [ShowIf("ifSelected")]
    public List<string> choices;
}

/*
public class SomeClass
{
    public void Process (){
    string firstQ = "q1";
        Base @base = new Base();
    }

    private Start()
    {
        var an = GetAnswer(null);

        foreach (var answer in an)
        {
            makeButn;
            makeButn.OnClick(selectedAns =>
            {
                var ab = getAnser(selectedAns);
            });
        }
    }


    public GetAnswer(string prev = null)
    {
        if (prev.IsNullOrEmpty)
        {
            var res = @base.Questions[prev];
            return res;
        }
    }
}


public static Base{
public Dictionary<int, Question> Questions = new Dictionary<int, Question>()
{
    new Question("q1",
        new List<Answer>()
        {
            new Answer("a1_1", "aaaa", "q2"),
            new Answer("a1_2", "1a2", "q3")
        },
    ),
    new Question("q2",
        new List<Answer>() {Answer.A("q2_a1", "1a1"), Answer.A("q2_a2", "1a2")},
        new List<string>() {"a1_1"}
    ),
    new Question("q3",
        new List<Answer>() {Answer.A("q3_a1", "1a1"), Answer.A("q3_a2", "1a2")},
    ),
    new Question("q4",
        new List<Answer>()
        {
            new Answer("a1_1", "aaaa", "q2"),
            new Answer("a1_2", "1a2", "q3")
        },
    ),
};
}

public struct Question
{
    public Question(int id, string question, List<Answer> answers)
    {
        QuestionText = question;
        Answers = answers;
        Id = id;
    }

    public string QuestionText { get; }
    public List<Answer> Answers { get; }
}


public struct Answer
{
    public string Id;
    public string Text;
    public string nextQuestionId;

    public Answer(string id, string text, string nextId)
    {
        Id = id;
        Text = text;
        nextQuestionId = nextId
    }

    public static Question GetNextQuestion()
    {
        return nextQuestionId;
    }

    // public static Answer A(string id, string text, string nextId) {
    //     return new Answer(id, text, nextId);
    // }
}

}*/