using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FindObjectsWithMissingComponents : Editor
{
    [MenuItem("Component/Find prefab with missing components")]
    public static void FindGameObjects()
    {
        var prefabPaths = AssetDatabase.GetAllAssetPaths().Where( path => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)).ToArray();
        GameObject parent = null;

        foreach (var path in prefabPaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var components = prefab.GetComponents<Component>();

            foreach (var component in components)
            {
                Debug.Log($"object: {path}");

                if (component == null)
                {
                    Debug.LogWarning($"object with missing component: {path}");
                    if (parent == null)
                    {
                        parent = new GameObject("Missing Component Objects");
                    }

                    var instance = Instantiate(prefab, parent.transform);
                    break;
                }
            }
        }
    }
    
    [MenuItem("Component/Find asset with missing components")]
    public static void FindAssets()
    {
        var prefabPaths = AssetDatabase.GetAllAssetPaths().Where( path =>path.Contains("Resources") && path.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase)).ToArray();
        
        foreach (var path in prefabPaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var components = prefab.GetComponents<Component>();

            foreach (var component in components)
            {
                Debug.Log($"object: {path}");
                
                if(component == null)
                    Debug.LogWarning($"object with missing component: {path}");
            }
        }
    }
}
