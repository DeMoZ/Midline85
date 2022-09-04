using System.Globalization;
using UnityEngine;

public class EntryRoot : MonoBehaviour
{
    private static EntryRoot _instance;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
        {
            Destroy(gameObject);
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
        
        Debug.Log($"[EntryRoot][time] Loading scene start.. {Time.realtimeSinceStartup}");
        
        CreateAppSettings();
        CreateRootEntity();
    }

    private void CreateAppSettings()
    {
        Application.targetFrameRate = 60;
        // CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        // Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void CreateRootEntity()
    {
        var rootEntity = new RootEntity(new RootEntity.Ctx
        {
            
        });
    }
}