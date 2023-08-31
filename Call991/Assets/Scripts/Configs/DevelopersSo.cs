using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(DevelopersSo), menuName = "Aa/Configs/" + nameof(DevelopersSo), order = 0)]
public class DevelopersSo : ScriptableObject
{
    [SerializeField] private List<DeveloperGroup> groups;
    public List<DeveloperGroup> Developers => groups;

    [Serializable]
    public class DeveloperGroup
    {
        public List<Developer> developers;
    }
    
    [Serializable]
    public class Developer
    {
        public string Position;
        public List<LocalizedString> NameKeys;
    }
}