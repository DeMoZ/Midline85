using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GameEntity : MonoBehaviour
{
    private static GameEntity _instance;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private VideoManager videoManager;
    
    [Space] 
    [SerializeField] private Image videoFade;
    [SerializeField] private Image screenFade;

    [Space]
    [SerializeField] private Transform clicksParent;

    private CompositeDisposable _disposable;
    
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

        _disposable = new CompositeDisposable();
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
        await Task.Delay(0);
    }

    private async void CreateRootEntity()
    {
        await Task.Delay(0);
        
        var blocker = new Blocker(screenFade, videoFade);
       
        var clickImage = Resources.Load<GameObject>("ClickPointImage");
        var clickPointHandler = new ClickPointHandler(clickImage, clicksParent).AddTo(_disposable);
        
        var rootEntity = new RootEntity(new RootEntity.Ctx
        {
            AudioManager = audioManager,
            VideoManager = videoManager,
            Blocker = blocker,
        }).AddTo(_disposable);
    }

    private void OnDestroy()
    {
        _disposable?.Dispose();
        ResourcesLoader.UnloadUnused();
    }
}