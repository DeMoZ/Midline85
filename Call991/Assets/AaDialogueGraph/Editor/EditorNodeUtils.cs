using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using Object = UnityEngine.Object;

namespace AaDialogueGraph.Editor
{
    public class EditorNodeUtils
    {
        public static string GetPathByObject(Object obj)
        {
            var path = string.Empty;
            if (obj != null)
            {
                path = UnityEditor.AssetDatabase.GetAssetPath(obj);

                if (!path.Contains(AaGraphConstants.AssetsResources))
                {
                    throw new Exception($"Selected object {path} not in the " +
                                        $"path {AaGraphConstants.AssetsResources} and will not be loaded in game");
                }

                path = path.Replace(AaGraphConstants.AssetsResources, "");
            }

            return path;
        }

        public static List<string> GetObjectPath(List<Object> objs)
        {
            return objs.Select(GetPathByObject).ToList();
        }
    }
}