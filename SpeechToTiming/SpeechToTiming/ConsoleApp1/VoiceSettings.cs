public class VoiceSettings
{
    public const string VoiceExtension = "wav";
    public const string TimingExtension = "asset";
    //public const string OutputRootPath = "/Users/roman/Git/_NADSAT/Call991/Call991/Assets/Resources/Phrases";
    public const string OutputRootPath = "/Users/roman/Git/_NADSAT/Call991/Timings";
    public const string OutputRootFolderBadFolder = "_TIMINGS";
    public const string Suffix = "_BAD";
    public const float AddTimeToLastWord = 1f;
    
    public static string GetLanguage()
    {
        return "ru-RU";
        //return "en-EN";
    }

    public static bool TryGetFolder(out string path)
    {
        // path = "/Users/roman/Git/_NADSAT/Call991/Call991/Call991_WwiseProject/Originals/Voices/Russian/pizza_george";
        // path = "/Users/roman/Git/_NADSAT/Call991/Call991/Call991_WwiseProject/Originals/Voices/LANG/PERSON_FOLDER";
        // path = "/Users/roman/Git/_NADSAT/Call991/Call991/Call991_WwiseProject/Originals/Voices/LANG/PERSON_FOLDER";
        
        //path = "/Users/roman/Git/_NADSAT/Call991/Call991/Call991_WwiseProject/Originals/Voices/Russian/pizza_george";
        //path = "/Users/roman/Git/_NADSAT/Call991/Call991/Call991_WwiseProject/Originals/Voices/Russian/pizza_gril";
        path = "/Users/roman/Git/_NADSAT/Call991/Call991/Call991_WwiseProject/Originals/Voices/Russian";

        // test folders
        // path = "/Users/roman/Git/_NADSAT/Call991/Voices/pizza.elena";
        // path = "/Users/roman/Git/_NADSAT/Call991/Voices/pizza.george";
        // path = "/Users/roman/Git/_NADSAT/Call991/Voices/pizza.girl";

        if (!Directory.Exists(path))
        {
            Console.WriteLine("The provided folder path is invalid or doesn't exist.");
            return false;
        }

        return true;
    }
}