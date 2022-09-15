using System;
using System.Globalization;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public class EntryRoot : MonoBehaviour
{
    private static EntryRoot _instance;

    private async void Awake()
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
        
        await CreateAppSettings();
        CreateRootEntity();
    }

    private async Task CreateAppSettings()
    {
        Application.targetFrameRate = 60;
        // CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        // Screen.sleepTimeout = SleepTimeout.NeverSleep;

        var cursorSettings = await ResourcesLoader.LoadAsync<CursorSet>("CursorSet");
        cursorSettings.ApplyCursor();
    }

    private void CreateRootEntity()
    {
        var rootEntity = new RootEntity(new RootEntity.Ctx
        {
            
        });
    }

    private void OnDestroy()
    {
        ResourcesLoader.UnloadUnused();
    }
}