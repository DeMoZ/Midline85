using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogPlace : MonoBehaviour
{
    [SerializeField] private ScreenPlace screenPlace = default;
    [SerializeField] private TextMeshProUGUI personName = default;
    [SerializeField] private TextMeshProUGUI description = default;

    public ScreenPlace ScreenPlace => screenPlace;

    public void Set(string name, string text, TextAppear appear, float appearDelay, float durationAfter, List<DialogueEvent> events)
    {
        
    }
    
    // public void PersonName(string name) => personName.text = name;
    // public void Description(string text) => description.text = name;

    public void RunText()
    {
        StartCoroutine("RunTextRoutine");
    }


    public void Clear()
    {
        personName.text = string.Empty;
        description.text = string.Empty;
    }
}