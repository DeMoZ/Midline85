using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

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

    public static string GetPathByObject(Object obj)
    {
        var path = string.Empty;
        if (obj != null)
        {
            path = UnityEditor.AssetDatabase.GetAssetPath(obj);
            path = path.Replace("Assets/Resources/", "");
        }

        return path;
    }
        
    public static List<string> GetObjectPath(List<Object> objs)
    {
        return objs.Select(GetPathByObject).ToList();
    }
}
