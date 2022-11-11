using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class EntryRoot : MonoBehaviour
{
    private static EntryRoot _instance;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private VideoManager videoManager;
    
    [Space] 
    [SerializeField] private Image videoFade;
    [SerializeField] private Image screenFade;

    private async void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Debug.Log($"[EntryRoot][time] Loading scene start.. {Time.realtimeSinceStartup}");

        await CreateAppSettings();
        CreateRootEntity();
    }

    private async Task CreateAppSettings()
    {
        Application.targetFrameRate = 60;
        // CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        // Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private async void CreateRootEntity()
    {
        var blocker = new Blocker(screenFade, videoFade);
        var cursorSettings = await ResourcesLoader.LoadAsync<CursorSet>("CursorSet");
        cursorSettings.ApplyCursor();
        
        var rootEntity = new RootEntity(new RootEntity.Ctx
        {
            audioManager = audioManager,
            videoManager = videoManager,
            blocker = blocker,
            cursorSettings = cursorSettings,
        });
    }

    private void OnDestroy()
    {
        ResourcesLoader.UnloadUnused();
    }
}