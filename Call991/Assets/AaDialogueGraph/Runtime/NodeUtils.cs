using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public static class NodeUtils
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
    
    public static async Task<T> GetObjectByPathAsync<T>(string path) where T : Object
    {
        var fileNameExtension = Path.GetFileName(path);
        var onlyFileName = Path.GetFileNameWithoutExtension(path);
        var split = path.Split(fileNameExtension);
        path = $"{split[0]}{onlyFileName}";
        
        var asset = await ResourcesLoader.LoadAsync<T>(path);
        return asset;
    }
}
