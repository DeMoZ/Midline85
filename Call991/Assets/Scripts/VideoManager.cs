using System;
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

    public void EnableVideoPlayer(bool enable)
    {
        videoPlayer.gameObject.SetActive(enable);
        vfxPlayer.gameObject.SetActive(enable);
        videoImage.gameObject.SetActive(enable);
        vfxImage.gameObject.SetActive(enable);
    }
    
    public void PlayVideo(VideoClip videoClip)
    {
        videoPlayer.clip = videoClip;
        videoPlayer.isLooping = true;
    }
    
    public void PlayVideo(EventVisualData data, VideoClip videoClip)
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