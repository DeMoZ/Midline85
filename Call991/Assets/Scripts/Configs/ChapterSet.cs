using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class ChapterSet : SerializedScriptableObject
{
    public string titleVideoSoName;
    public string levelVideoSoName;
    public List<Dialogues> dialogues;
    
    public async Task<Dialogues> LoadDialogues(Language language, string lvl)
    {
        var newDialogues = ScriptableObject.CreateInstance<Dialogues>();
        newDialogues.phrases = new List<PhraseSet>();

        foreach (var dialogue in dialogues)
            newDialogues.phrases.AddRange(dialogue.phrases);

        var path = $"{language}/{lvl}/";

        foreach (var phraseSet in newDialogues.phrases)
            phraseSet.Phrase = await LoadTextPhrase(path, phraseSet.phraseId);

        ResourcesLoader.UnloadUnused();
        return newDialogues;
    }

    private async Task<Phrase> LoadTextPhrase(string path, string phraseId)
    {
        var file = Path.Combine(path, phraseId);
        var phrase = await ResourcesLoader.LoadAsync<Phrase>(file);

        if (phrase == null)
        {
            Debug.LogError($"[{this}] Not found phrase asset {Path.Combine(path, phraseId)}; loaded default");
            phrase = ScriptableObject.CreateInstance<Phrase>();
            phrase.phraseId = phraseId;
            phrase.text = $"{phraseId}: No Text found for phrase.";
            phrase.totalTime = 3f;
            phrase.SeparatePhrase();
        }

        return phrase;
    }
}

[Serializable]
public class Achievement
{
    public Sprite sprite;
    [Space]
    public LocalizedString descriptionTopKey;
    public Dictionary<string, bool> requirements;
}