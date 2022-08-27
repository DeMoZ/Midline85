using UnityEngine;

public class EntryRoot : MonoBehaviour
{
    private static EntryRoot _instance;
    private static RootEntity _rootEntityInstance; 

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (_rootEntityInstance != null)
        {
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
    }

    private void CreateRootEntity()
    {
        _rootEntityInstance = new RootEntity(new RootEntity.Ctx
        {
            
        });
    }
}