// https://console.cloud.google.com/speech/
// cloud.google servise to translate audio file to text SpeechToText

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
        
        foreach (var voiceFile in files)
        {
            var isTimingAlreadyExists = Utilities.IsFileExits(voiceFile, out var timingFile);
            if (isTimingAlreadyExists) continue;
            
            var dataReceived =  speechToText.TryGetTextDataFromVoice(voiceFile, out var voiceData);
            if (!dataReceived) continue;
            
            // create yaml string from voiceData
            var yaml = TextToYaml.GetYamlDataFromVoiceData(voiceData);
            
            // Save the YAML to a file
            File.WriteAllText(timingFile, yaml);
        }
    }
}