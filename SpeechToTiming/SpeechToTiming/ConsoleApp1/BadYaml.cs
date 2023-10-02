using Google.Protobuf.WellKnownTypes;

public static class BadYaml
{
    private const string NotRecognisedFilePhrase = "<color=red>Voice file was not recognised</color>";
    private static readonly Duration ZeroSeconds = new Duration { Seconds = 0, Nanos = 0 };
    private static readonly Duration SomeSeconds = new Duration { Seconds = 4, Nanos = 0 };

    public static VoiceData GetBadData(string fileName)
    {
        var newName = Path.GetFileNameWithoutExtension(fileName);
        var newFileName = $"{newName}{VoiceSettings.Suffix}";
        return new VoiceData
        {
            FileName = newFileName,
            Phrase = NotRecognisedFilePhrase,
            Words = new List<WordData>
            {
                new()
                {
                    Word = NotRecognisedFilePhrase,
                    StartTime = ZeroSeconds,
                    EndTime = SomeSeconds,
                    Diration = SomeSeconds,
                },
            },
            BeforeFirstWord = ZeroSeconds,
            AfterLastWord = ZeroSeconds,
            TotalTime = SomeSeconds,
        };
    }

    public static string GetBadFileName(string filePath)
    {
        var path = Path.GetDirectoryName(filePath);
        var name = Path.GetFileNameWithoutExtension(filePath);
        var newName = Path.Combine(path, $"{name}{VoiceSettings.Suffix}.{VoiceSettings.TimingExtension}");
        return newName;
    }
}