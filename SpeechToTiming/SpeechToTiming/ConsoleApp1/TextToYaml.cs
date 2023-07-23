using Google.Protobuf.WellKnownTypes;
using YamlDotNet.Serialization;

public static class TextToYaml
{
    public static string GetYamlDataFromVoiceData(VoiceData voiceData)
    {
        var wordTimes = new List<WordTimeData>();
        foreach (var wordData in voiceData.Words)
        {
            var word = new WordTimeData
            {
                word = wordData.Word,
                wipe = false,
                timeLine = DurationToFloat(wordData.StartTime),
                time = DurationToFloat(wordData.Diration)
            };
            
            wordTimes.Add(word);
        }

        var beforeFirstWord = DurationToFloat(voiceData.BeforeFirstWord); // 0.033,
        var afterLastWord = DurationToFloat(voiceData.AfterLastWord); // 1.6,
        var totalTime = DurationToFloat(voiceData.TotalTime); // 1.633

        var yamlData = new YamlData
        {
            data = new MonoBehaviourData
            {
                m_ObjectHideFlags = 0,
                m_CorrespondingSourceObject = new Dictionary<string, object> { { "fileID", 0 } },
                m_PrefabInstance = new Dictionary<string, object> { { "fileID", 0 } },
                m_PrefabAsset = new Dictionary<string, object> { { "fileID", 0 } },
                m_GameObject = new Dictionary<string, object> { { "fileID", 0 } },
                m_Enabled = 1,
                m_EditorHideFlags = 0,
                m_Script = new Dictionary<string, object>
                {
                    { "fileID", 11500000 },
                    { "guid", "810af83c7a0b40e9b11bcb70e2ae7a42" },
                    { "type", 3 }
                },
                m_Name = voiceData.FileName,
                phraseId = voiceData.FileName,
                overrideSoundName = false,
                soundFileName = string.Empty,
                text = voiceData.Phrase,
                wordTimes = wordTimes,
                beforeFirstWord = beforeFirstWord,
                afterLastWord = afterLastWord,
                totalTime = totalTime
            }
        };
        
        var serializer = new SerializerBuilder().Build();
        var yaml = serializer.Serialize(yamlData);
        FixYamlVersion(ref yaml);
        
        return yaml;
    }
    
    private static float DurationToFloat(Duration duration)
    {
        if (duration == null) return 0;
        
        return (float)duration.ToTimeSpan().TotalSeconds;
    }

    private static void FixYamlVersion(ref string yaml)
    {
        /*yaml = yaml.Replace(@"YAMLVersion: '%YAML 1.1'
        TAG: '%TAG !u! tag:unity3d.com,2011:'
        data:",@"%YAML 1.1
               %TAG !u! tag:unity3d.com,2011:
        --- !u!114 &11400000
        MonoBehaviour:");*/

        yaml = yaml.Replace(@"YAMLVersion: '%YAML 1.1'", @"%YAML 1.1");
        yaml = yaml.Replace(@"TAG: '%TAG !u! tag:unity3d.com,2011:'",
            "%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000");
        yaml = yaml.Replace(@"data:", @"MonoBehaviour:");

    }
}

// YAML CLASSES
public class MonoBehaviourData
{
    public int m_ObjectHideFlags { get; set; }
    public Dictionary<string, object> m_CorrespondingSourceObject { get; set; }
    public Dictionary<string, object> m_PrefabInstance { get; set; }
    public Dictionary<string, object> m_PrefabAsset { get; set; }
    public Dictionary<string, object> m_GameObject { get; set; }
    public int m_Enabled { get; set; }
    public int m_EditorHideFlags { get; set; }
    public Dictionary<string, object> m_Script { get; set; }
    public string m_Name { get; set; }
    public string m_EditorClassIdentifier { get; set; }
    public string phraseId { get; set; }
    public bool overrideSoundName { get; set; }
    public string soundFileName { get; set; }
    public string text { get; set; }
    public List<WordTimeData> wordTimes { get; set; }
    public float beforeFirstWord { get; set; }
    public float afterLastWord { get; set; }
    public float totalTime { get; set; }
}

public class WordTimeData
{
    public bool wipe { get; set; }
    public string word { get; set; }
    public float timeLine { get; set; }
    public float time { get; set; }
}

public class YamlData
{
    public string YAMLVersion { get; } = "%YAML 1.1";
    public string TAG { get; } = "%TAG !u! tag:unity3d.com,2011:";
    public MonoBehaviourData data { get; set; }
}