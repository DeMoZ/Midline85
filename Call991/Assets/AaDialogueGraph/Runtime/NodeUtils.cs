using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class NodeUtils
{
    public const string AssetsResources = "Assets/Resources/";

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
         
            if (!path.Contains(AssetsResources))
                throw new Exception($"Selected object {path} not in the path {AssetsResources} and will not be loaded in game");
            
            path = path.Replace(AssetsResources, "");
        }

        return path;
    }
        
    public static List<string> GetObjectPath(List<Object> objs)
    {
        return objs.Select(GetPathByObject).ToList();
    }
}
