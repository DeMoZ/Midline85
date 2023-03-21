using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class NodeUtils
{
    public static T GetObjectByPath<T>(string path) where T : Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        return asset;
    }

    public static string GetPathByObject(Object obj)
    {
        return obj != null ? AssetDatabase.GetAssetPath(obj) : string.Empty;
    }
        
    public static List<string> GetObjectPath(List<Object> objs)
    {
        return objs.Select(GetPathByObject).ToList();
    }
}
