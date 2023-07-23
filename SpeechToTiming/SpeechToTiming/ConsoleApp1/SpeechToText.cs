using System.Text;
using Google.Cloud.Speech.V1;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;

public class SpeechToText
{
    private readonly RecognitionConfig _config;
    private readonly SpeechClient _client;

    public SpeechToText(string language)
    {
        _config = new RecognitionConfig
        {
            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
            SampleRateHertz = 48000,
            AudioChannelCount = 1,
            Model = "latest_long",
            LanguageCode = language,
            EnableAutomaticPunctuation = true,
            EnableSpokenPunctuation = true,
            MaxAlternatives = 5,
            EnableWordTimeOffsets = true
        };

        _client = SpeechClient.Create();
    }

    public bool TryGetTextDataFromVoice(string voiceFile, out VoiceData voiceData)
    {
        var audio = RecognitionAudio.FromStream(File.OpenRead(voiceFile));
        var response = _client.Recognize(_config, audio);
        voiceData = new VoiceData
        {
            FileName = Path.GetFileNameWithoutExtension(voiceFile),
            Words = new List<WordData>(),
        };

        foreach (var result in response.Results)
        {
            // for (var index = 0; index < result.Alternatives.Count; index++)
            // {
            var alt = result.Alternatives[0];
            var buildString = new StringBuilder();
            voiceData.Phrase = alt.Transcript;

            buildString.Append($"Transcript: {alt.Transcript}");
            var lastStart = new Duration();
            var lastEnd = new Duration();

            foreach (var word in alt.Words)
            {
                voiceData.BeforeFirstWord ??= word.StartTime;
                  
                lastStart = word.StartTime;
                lastEnd = word.EndTime;
                buildString.Append($"\n - Word: {word.Word} ; start {word.StartTime}; end {word.EndTime} " +
                                   $"; duration = {word.EndTime - word.StartTime}");
                voiceData.Words.Add(new WordData
                {
                    Word = word.Word,
                    StartTime = word.StartTime,
                    EndTime = word.EndTime,
                    Diration = word.EndTime - word.StartTime,
                });
            }

            voiceData.TotalTime = result.ResultEndTime;

            buildString.Append($"\n - after last word {lastEnd - lastStart}");

            Console.WriteLine(buildString);
            //}
        }

        Console.WriteLine($"result json = {JsonConvert.SerializeObject(voiceData)}");
        return true;
    }
}

[Serializable]
public class VoiceData
{
    public string FileName;
    public string Phrase;
    public List<WordData> Words;
    public Duration BeforeFirstWord;
    public Duration AfterLastWord;
    public Duration TotalTime;
}

[Serializable]
public class WordData
{
    public string Word;
    public Duration StartTime;
    public Duration EndTime;
    public Duration Diration;
}