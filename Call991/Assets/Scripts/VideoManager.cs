using System;
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

    public async Task PrepareVideo(string sceneVideoUrl)
    {
        videoPlayer.Stop();
        videoPlayer.url = sceneVideoUrl;
        videoPlayer.Prepare();
        
        while (!videoPlayer.isPrepared)
            await Task.Yield();

        PlayPreparedVideo();
    }

    private void PlayPreparedVideo()
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

    public void Enable(bool isActive)
    {
        videoPlayer.gameObject.SetActive(isActive);
        vfxPlayer.gameObject.SetActive(isActive);
        videoImage.gameObject.SetActive(isActive);
        vfxImage.gameObject.SetActive(isActive);
    }
}