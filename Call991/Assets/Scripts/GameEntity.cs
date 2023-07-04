using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GameEntity : MonoBehaviour
{
    private static GameEntity _instance;
    [SerializeField] private WwiseAudio wwisePrefab;
    [SerializeField] private VideoManager videoManager;

    [Space] [SerializeField] private Image videoFade;
    [SerializeField] private Image screenFade;

    [Space] [SerializeField] private Transform clicksParent;

    private CompositeDisposable _disposables;

    private void Awake()
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

        _disposables = new CompositeDisposable();
        DontDestroyOnLoad(gameObject);

        Debug.Log($"[EntryRoot][time] Loading scene start.. {Time.realtimeSinceStartup}");

        
    }

    private async void Start()
    {
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
        
        // check for the test dialogue.
        var testDialogue = FindObjectOfType<TestDialogue>();
        var overridenDialogue = testDialogue 
            ? testDialogue.GetDialogue() 
            : new OverridenDialogue(false,false,false,null);

        var audioManager = Instantiate(wwisePrefab, transform);
        
        var rootEntity = new RootEntity(new RootEntity.Ctx
        {
            AudioManager = audioManager,
            VideoManager = videoManager,
            VideoFade = videoFade,
            ScreenFade = screenFade,
            ClicksParent = clicksParent,
            OverridenDialogue = overridenDialogue,
        }).AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _disposables?.Dispose();
        ResourcesLoader.UnloadUnused();
    }
}