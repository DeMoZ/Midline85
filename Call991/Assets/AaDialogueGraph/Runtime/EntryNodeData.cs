using System;
using System.Collections.Generic;

namespace AaDialogueGraph
{
    [Serializable]
    public class EntryNodeData : AaNodeData
    {
        public string LevelId;
        public List<string> Languages = new();
        public string ButtonFilter;
        public string SoundAsset;
    }
}