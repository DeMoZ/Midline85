using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

[Serializable]
public class Achievement
{
    public Sprite sprite;
    [Space]
    public LocalizedString descriptionTopKey;
    public Dictionary<string, bool> requirements;
}