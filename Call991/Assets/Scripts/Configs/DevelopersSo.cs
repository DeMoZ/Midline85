using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(DevelopersSo), menuName = "Aa/Configs/" + nameof(DevelopersSo), order = 0)]

public class DevelopersSo : ScriptableObject
{
    [SerializeField] private List<Developer> developers;
    public List<Developer> Developers => developers;

    [Serializable]
    public class Developer
    {
        public string Position;
        public LocalizedString NameKey;
    }
}