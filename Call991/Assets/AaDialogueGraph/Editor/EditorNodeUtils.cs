using System;
using System.Collections.Generic;
using System.Linq;
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

        public static List<string> GetPathByObjects(List<Object> objs)
        {
            return objs.Select(GetPathByObject).ToList();
        }

        public static List<string> GetButtons(string filtersInLine)
        {
            if (string.IsNullOrEmpty(filtersInLine)) return AaKeys.ChoiceKeys; 
            
            var buttons = new List<string>();
            var filters = filtersInLine.Split(";").ToList();

            foreach (var filter in filters)
            {
                var mutches = AaKeys.ChoiceKeys.Where(b => b.Contains(filter)).ToList();
                if (mutches.Count > 0)
                {
                    buttons.AddRange(mutches);
                }
            }

            return buttons;
        }
    }
}