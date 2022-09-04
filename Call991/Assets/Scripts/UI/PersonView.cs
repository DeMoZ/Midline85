using TMPro;
using UnityEngine;

public class PersonView : MonoBehaviour
{
    [SerializeField] private ScreenPlace screenPlace = default;
    [SerializeField] private TextMeshProUGUI personName = default;
    [SerializeField] private TextMeshProUGUI description = default;

    public ScreenPlace ScreenPlace => screenPlace;

    public void ShowPhrase(Phrase phrase)
    {
        description.text = "";

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        personName.text = phrase.GetPersonName();
        description.text = phrase.description;
    }

    // public void PersonName(string name) => personName.text = name;
    // public void Description(string text) => description.text = name;

    public void HidePhrase() =>
        description.gameObject.SetActive(false);

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