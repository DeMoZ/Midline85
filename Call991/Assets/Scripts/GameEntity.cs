using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class GameEntity : MonoBehaviour
{
    private static GameEntity _instance;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private VideoManager videoManager;

    [Space] [SerializeField] private Image videoFade;
    [SerializeField] private Image screenFade;

    [Space] [SerializeField] private Transform clicksParent;

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

        var onScreenFade = new ReactiveCommand<(bool show, float time)>();
        var objectEvents = new ObjectEvents(new ObjectEvents.Ctx
        {
            OnScreenFade = onScreenFade,
        }).AddTo(_disposable);

        var blocker = new Blocker(screenFade, videoFade, onScreenFade);

        var clickImage = Resources.Load<GameObject>("ClickPointImage");
        var clickPointHandler = new ClickPointHandler(clickImage, clicksParent).AddTo(_disposable);

        // check for the test dialogue.
        var testDialogue = FindObjectOfType<TestDialogue>();
        var overridenDialogue = testDialogue ? testDialogue.GetDialogue() : null;

        var rootEntity = new RootEntity(new RootEntity.Ctx
        {
            AudioManager = audioManager,
            VideoManager = videoManager,
            Blocker = blocker,
            ObjectEvents = objectEvents,
            OverridenDialogue = overridenDialogue,
        }).AddTo(_disposable);
    }

    private void OnDestroy()
    {
        _disposable?.Dispose();
        ResourcesLoader.UnloadUnused();
    }
}