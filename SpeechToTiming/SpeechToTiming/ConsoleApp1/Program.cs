// https://console.cloud.google.com/speech/
// cloud.google servise to translate audio file to text SpeechToText

using System.Text;

class Program
{
    private static string _filePath;

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        // TestTextData.OneFileGetTimingData();
        var language = VoiceSettings.GetLanguage();
        var isFolderExist = VoiceSettings.TryGetFolder(out _filePath);
        if (!isFolderExist) return;

        var gotFiles = Utilities.TryGetFiles(_filePath, out var files);
        if (!gotFiles) return;

        var outputFolder = Utilities.GetOutputFolder(_filePath);
        var speechToText = new SpeechToText(VoiceSettings.GetLanguage());
        var badResults = new StringBuilder();
        badResults.Append("Not recognised list:");

        foreach (var voiceFile in files)
        {
            var isTimingAlreadyExists = Utilities.IsFileExits(voiceFile, out var timingFile);
            if (isTimingAlreadyExists) continue;

            var dataReceived = speechToText.TryGetTextDataFromVoice(voiceFile, out var voiceData);
            if (!dataReceived)
            {
                badResults.Append($"\n - {voiceFile}");
                voiceData = BadYaml.GetBadData(voiceData.FileName);
                timingFile = BadYaml.GetBadFileName(timingFile);
            }

            var yaml = TextToYaml.GetYamlDataFromVoiceData(voiceData);
            File.WriteAllText(timingFile, yaml);
        }

        if (badResults.Length > 1)
            Console.WriteLine(badResults);
    }
}