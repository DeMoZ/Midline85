using System.Collections.Generic;
using AaDialogueGraph;

public class LevelData
{
    public Dictionary<string, AaNodeData> Nodes { get; private set; }
    public List<NodeLinkData> Links { get; private set; }

    public LevelData(Dictionary<string, AaNodeData> nodes, List<NodeLinkData> links)
    {
        Nodes = nodes;
        Links = links;
    }

    public EntryNodeData GetEntryNode()
    {
        foreach (var node in Nodes)
        {
            if (node.Value is EntryNodeData data)
            {
                return data;
            }
        }

        throw new System.Exception("Entry Node not found in data file");
    }
}