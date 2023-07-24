public static class Utilities
{
    private static string _outputPath;
    public static bool TryGetFiles(string path, out string[] files)
    {
        files = Directory.GetFiles(path, $"*.{VoiceSettings.VoiceExtension}");

        if (files.Length == 0)
        {
            Console.WriteLine($"No files with the extension '.{VoiceSettings.VoiceExtension}' were found in the folder.");
            return false;
        }

        Console.WriteLine($"Files amount {files.Length} with the extension '.{VoiceSettings.VoiceExtension}' found in the folder:");
        foreach (var file in files) Console.WriteLine(file);

        Console.WriteLine("end list");
        return true;
    }
    
    public static string GetOutputFolder(string voicesPath)
    {
        //voicesPath = "/Users/roman/Git/_NADSAT/Call991/Call991/Call991_WwiseProject/Originals/Voices/LANG/PERSON_FOLDER";
        var folders = voicesPath.Split("/");
        
        if (folders.Length == 0)
        {
            Console.WriteLine("Given folder dont has good structure but timings will be taken from voices.");
            _outputPath = $"{voicesPath}/{VoiceSettings.OutputRootFolderBadFolder}";
        }
        else
        {
            _outputPath = $"{VoiceSettings.OutputRootPath}/{folders[^1]}";
        }
        
        if (!Directory.Exists(_outputPath))
        {
            try
            {
                Directory.CreateDirectory(_outputPath);
                Console.WriteLine($"Output Folder created successfully. {_outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating the folder: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Output Folder already exists. {_outputPath}");
        }

        return _outputPath;
    }

    public static bool IsFileExits(string voiceFile, out string resultFilePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(voiceFile);
        var resultFile = $"{fileName}.{VoiceSettings.TimingExtension}";
        resultFilePath = Path.Combine(_outputPath, resultFile);

        if (File.Exists(resultFilePath))
        {
            Console.WriteLine($"timing file already exists. {resultFilePath}");
            return true;
        }

        return false;
    }
}