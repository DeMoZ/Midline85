using Configs;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class PhraseSfxEventSo : PhraseEventSo
{
    public bool clip;
    [ShowIf("clip")]
    public AudioClip audioClip;

    [ShowIf("AudioName")] public string audioName;
    private bool AudioName => !clip;
}