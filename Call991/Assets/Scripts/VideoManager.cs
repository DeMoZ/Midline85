using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AaDialogueGraph;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class VideoSet
{
    public PhraseEventLayer Layer;
    public float Delay;
    public bool Loop;
    public bool Stop;
    public VideoClip Clip;
}

public class VideoManager : MonoBehaviour
{
    public struct Ctx
    {
    }

    [SerializeField] private List<VideoPlayer> videoPlayers = default;

    private Ctx _ctx;
    private string _currentVideoPath;
    private VideoClip _currentVideoClip;

    public void SetCtx(Ctx ctx)
    {
        _ctx = ctx;
    }

    private async Task PrepareVideo()
    {
        await Task.Delay(1);
        // videoPlayer.Stop();
        // videoPlayer.Prepare();
        //
        // while (!videoPlayer.isPrepared)
        //     await Task.Yield();
    }

    public void PlayPreparedVideo()
    {
        // videoPlayer.Play();
        // videoPlayer.isLooping = true;
    }

    public void StopPlayers()
    {
        foreach (var player in videoPlayers)
        {
            StopVideo(player);
        }
    }
    
    public void PlayVideo(VideoSet data)
    {
        var layer = Mathf.Clamp((int)data.Layer, 0, videoPlayers.Count - 1);
        var player = videoPlayers[layer];

        if (data.Stop)
        {
            StopVideo(player);
        }
        else
        {
            player.clip = data.Clip;
            player.isLooping = data.Loop;
            player.gameObject.SetActive(true);
            player.Play();
        }
    }

    public void PlayVideo(EventVisualData data, VideoClip videoClip)
    {
        var layer = Mathf.Clamp((int)data.Layer, 0, videoPlayers.Count - 1);
        var player = videoPlayers[layer];

        if (data.Stop)
        {
            StopVideo(player);
        }
        else
        {
            player.clip = videoClip;
            player.isLooping = data.Loop;
            player.gameObject.SetActive(true);
            player.Play();
        }
    }
    
    private void StopVideo(VideoPlayer player)
    {
        Debug.Log($"[{this}] Stop videoPlayer {player.gameObject.name}");
        player.Stop();
        player.gameObject.SetActive(false);
        player.clip = null;
    }
}