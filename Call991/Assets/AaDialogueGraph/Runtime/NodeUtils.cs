using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

public class NodeUtils
{
    public static T GetObjectByPath<T>(string path) where T : Object
    {
        var fileNameExtension = Path.GetFileName(path);
        var onlyFileName = Path.GetFileNameWithoutExtension(path);
        var split = path.Split(fileNameExtension);
        path = $"{split[0]}{onlyFileName}";
        
        var asset = Resources.Load<T>(path);
        return asset;
    }
}
