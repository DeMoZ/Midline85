using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class WordTime
{
    [TableColumnWidth(40, false)] [Tooltip("wipe previous text and place this word")]
    public bool wipe;

    [TableColumnWidth(100)] public string word;
    
    [TableColumnWidth(60, false)]
    public float timeLine = 0.4f;

    [TableColumnWidth(60, false)] [ReadOnly]
    public float time = 0.4f;
}