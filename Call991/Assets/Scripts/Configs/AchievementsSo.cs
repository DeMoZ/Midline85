using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu]
public class AchievementsSo : SerializedScriptableObject
{
    [Space]
    [OdinSerialize] [NonSerialized] public List<Achievement> achievements;
}