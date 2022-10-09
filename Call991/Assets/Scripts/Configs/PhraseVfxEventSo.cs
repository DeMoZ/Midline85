using Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu]
public class PhraseVfxEventSo : PhraseEventSo
{
    public bool clip;
    [ShowIf("clip")]
    public VideoClip videoClip;

    [ShowIf("VideoName")] public string videoName;
    private bool VideoName => !clip;
}