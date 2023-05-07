using System;
using System.IO;
using System.Threading.Tasks;
using AaDialogueGraph;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public struct Ctx
    {
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
        _ctx = ctx;
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
        var conf = await ResourcesLoader.LoadAsync<PhraseVfxEventSo>(eventId);
        return conf;
    }

    public void PlayEventVideo(EventVisualData data, VideoClip videoClip)
    {
        // TODO layers should be added to mixer
        switch (data.Layer)
        {
            case PhraseEventLayer.Effects: // case PhraseEventTypes.LoopSfx:
                videoPlayer.clip = videoClip;
                videoPlayer.isLooping = data.Loop;
                throw new NotImplementedException();
                break;
            case PhraseEventLayer.Single1: // case PhraseEventTypes.Video:
                videoPlayer.clip = videoClip;
                videoPlayer.isLooping = data.Loop;
                break;
            case PhraseEventLayer.Single2: //case PhraseEventTypes.Video: should be second video layer
                videoPlayer.clip = videoClip;
                videoPlayer.isLooping = data.Loop;
                throw new NotImplementedException();
                break;
            case PhraseEventLayer.Multiple: // on that layer can be several musics at the same time
                videoPlayer.clip = videoClip;
                videoPlayer.isLooping = data.Loop;
                throw new NotImplementedException();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}