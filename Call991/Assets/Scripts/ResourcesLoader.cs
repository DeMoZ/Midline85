using System.Threading.Tasks;
using UnityEngine;

public static class ResourcesLoader
{
    public static async Task<T> LoadAsync<T>(string assetName) where T : UnityEngine.Object
    {
        ResourceRequest resource = Resources.LoadAsync<T>(assetName);
        while (!resource.isDone)
        {
            await Task.Yield();
        }

        return resource.asset as T;
    }
}