using Google.Cloud.Speech.V1;
using Google.Protobuf.WellKnownTypes;

public static class TestTextData
{
    public static void OneFileGetTimingData()
    {
        //string audioFile = "audiofile.wav";
        //string audioFile = "/Users/roman/Git/_NADSAT/Call991/Voices/audiofile.wav";
        string audioFile = "/Users/roman/Git/_NADSAT/Call991/Voices/pizza.elena/pizza.elena.001.wav";
        bool timings = GetTimings(audioFile, "ru-RU");
        Console.WriteLine(timings);
    }
    
    public static bool GetTimings(string fileName, string language)
    {
        var client = SpeechClient.Create();

        //var content = ByteString.FromStream(File.OpenRead(fileName));
        var audio = RecognitionAudio.FromStream(File.OpenRead(fileName));

        var config = new RecognitionConfig
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

        // Sends the request to Google to transcribe the audio
        var response = client.Recognize(config, audio);
        foreach (var result in response.Results)
        {
            foreach (var alt in result.Alternatives)
            {
                Console.WriteLine("Transcript: {0}", alt.Transcript);
                Duration lastStart = new Duration();
                Duration lastEnd = new Duration();
                foreach (var word in alt.Words)
                {
                    lastStart = word.StartTime;
                    lastEnd = word.EndTime;
                    Console.WriteLine($"  Word: {word.Word} ; start {word.StartTime}; end {word.EndTime} " +
                                      $"; duration = {word.EndTime - word.StartTime}");
                }

                Console.WriteLine($"after last word {lastEnd - lastStart}");
            }
        }

        return true;
    }
}