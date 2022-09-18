using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class MakePhrasesScriptableObject : ScriptableObject
{
    public CompositeDialogue copyFromDialogues = default;
    public Language Language = default;
    public string Lvl = "7_lvl";

    [Button("Make Assets/Resources/[Language]/[Lvl]/")]
    public void CreateMyAsset()
    {
        foreach (var dialogue in copyFromDialogues.dialogues)
        {
            foreach (var phrase in dialogue.phrases)
            {
                var asset = ScriptableObject.CreateInstance<Phrase>();
                asset.phraseId = phrase.phraseId;
                //asset.text = phrase.text;
                asset.SeparatePhrase();
                //asset.wordTimes = phrase.wordTime;

                AssetDatabase.CreateAsset(asset, $"Assets/Resources/{Language}/{Lvl}/{phrase.phraseId}.asset");
                AssetDatabase.SaveAssets();
            }
        }

        EditorUtility.FocusProjectWindow();
        //Selection.activeObject = asset;
    }
}

public static class StringExtension
{
    public static string DotToDash(this string input)
    {
        return input.Replace('.', '_');
    }

    public static string DashToDot(this string input)
    {
        return input.Replace('_', '.');
    }
}