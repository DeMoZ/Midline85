using System;
using System.IO;
using System.Threading.Tasks;
using Configs;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public struct Ctx
    {
        public GameSet gameSet;
        public string videoPath;

        public string levelFolder;
    }

    [SerializeField] private VideoPlayer videoPlayer = default;
    [SerializeField] private VideoPlayer vfxPlayer = default;

    [SerializeField] private RawImage videoImage = default;
    [SerializeField] private RawImage vfxImage = default;

    private Ctx _ctx;
    private string _currentVideoPath;
    private VideoClip _currentVideoClip;

    public void SetCtx(Ctx ctx)
    {
    }

    public async Task LoadVideoSoToPrepareVideo(string eventId)
    {
        var config = await LoadConfig(eventId);
        if (config == null)
        {
            Debug.LogError($"[{this}] sound event SO wasn't found: A PATH /{eventId}");
            return;
        }

        if (config.clip)
        {
            await PrepareVideo(config.videoClip);
        }
        else
        {
            var streamingPath = "file:///" + Path.Combine(Application.streamingAssetsPath, "Videos/EventVideos");
            var videoPath = Path.Combine(streamingPath, config.videoName + ".mp4");
            await PrepareVideo(videoPath);
        }
    }

    public async Task PrepareVideo(string url)
    {
        videoPlayer.Stop();
        videoPlayer.clip = null;
        videoPlayer.url = url;

        await PrepareVideo();
    }

    public async Task PrepareVideo(VideoClip clip)
    {
        videoPlayer.Stop();
        videoPlayer.url = null;
        videoPlayer.clip = clip;

        await PrepareVideo();
    }

    private async Task PrepareVideo()
    {
        videoPlayer.Stop();
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            await Task.Yield();
    }

    public void PlayPreparedVideo()
    {
        videoPlayer.Play();
        videoPlayer.isLooping = true;
    }

    public void PlayVideo(string sceneVideoUrl, PhraseEventTypes phraseEventTypes)
    {
        switch (phraseEventTypes)
        {
            case PhraseEventTypes.Video:
                break;
            case PhraseEventTypes.Vfx:
                vfxPlayer.url = sceneVideoUrl;
                vfxPlayer.isLooping = false;
                break;
            case PhraseEventTypes.LoopVfx:
                videoPlayer.url = sceneVideoUrl;
                videoPlayer.isLooping = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(phraseEventTypes), phraseEventTypes, null);
        }
    }

    public void PlayVideo(VideoClip clip, PhraseEventTypes phraseEventTypes)
    {
        switch (phraseEventTypes)
        {
            case PhraseEventTypes.Video:
                break;
            case PhraseEventTypes.Vfx:
                vfxPlayer.clip = clip;
                videoPlayer.isLooping = false;
                break;
            case PhraseEventTypes.LoopVfx:
                videoPlayer.clip = clip;
                videoPlayer.isLooping = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(phraseEventTypes), phraseEventTypes, null);
        }
    }

    public void EnableVideo(bool enable)
    {
        videoImage.gameObject.SetActive(enable);
    }

    public void EnableVideoPlayer(bool isActive)
    {
        videoPlayer.gameObject.SetActive(isActive);
        vfxPlayer.gameObject.SetActive(isActive);
        videoImage.gameObject.SetActive(isActive);
        vfxImage.gameObject.SetActive(isActive);
    }

    private async Task<PhraseVfxEventSo> LoadConfig(string eventId)
    {
        //var soFile = Path.Combine(_ctx.eventSoPath, eventId);
        var conf = await ResourcesLoader.LoadAsync<PhraseVfxEventSo>(eventId);
        return conf;
    }
}