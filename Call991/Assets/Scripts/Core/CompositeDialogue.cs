using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu]
public class CompositeDialogue : ScriptableObject
{
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
            phrase.popTime = 3f;
            phrase.SeparatePhrase();
        }
        
        return phrase;
    }
}