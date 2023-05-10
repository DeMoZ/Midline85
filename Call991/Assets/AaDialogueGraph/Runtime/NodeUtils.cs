using System.IO;
using System.Text;
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
        var resourcePath = new StringBuilder();
        resourcePath.Append(split[0]);
        resourcePath.Append(onlyFileName);
        
        var asset = Resources.Load<T>(resourcePath.ToString());
        return asset;
    }
    
    public static async Task<T> GetObjectByPathAsync<T>(string path) where T : Object
    {
        var fileNameExtension = Path.GetFileName(path);
        var onlyFileName = Path.GetFileNameWithoutExtension(path);
        var split = path.Split(fileNameExtension);
        var resourcePath = new StringBuilder();
        resourcePath.Append(split[0]);
        resourcePath.Append(onlyFileName);
        
        var asset = await ResourcesLoader.LoadAsync<T>(resourcePath.ToString());
        return asset;
    }
}
