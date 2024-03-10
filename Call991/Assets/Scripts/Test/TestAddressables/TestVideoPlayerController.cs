using System.Threading.Tasks;
using ContentDelivery;
using Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TestVideoPlayerController : MonoBehaviour
{
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private Button _button;
    [SerializeField] private string _videoName;
    [SerializeField] private Slider _progress;

    private IAddressableDownloader _downloader;

    private void Awake()
    {
        _downloader = new AddressableDownloader();
        _button.onClick.AddListener(PlayVideo);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(PlayVideo);
    }

    private void PlayVideo()
    {
        PlayVideoAsync().Forget();
    }

    private async Task PlayVideoAsync()
    {
        var clip = await _downloader.DownloadAsync<VideoClip>(_videoName, OnProgress,destroyCancellationToken);
        if (clip != null)
        {
            _videoPlayer.clip = clip;
            _videoPlayer.Play();
        }
    }

    private void OnProgress(float value)
    {
        _progress.value = value;
        Debug.Log($"{nameof(TestVideoPlayerController)} Download progress {value}");
    }
}