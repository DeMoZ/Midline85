using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AaDialogueGraph;
using UnityEngine;
using UnityEngine.UI;
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
    
    private List<bool> _pausedPlayers;

    private Ctx _ctx;

    public void SetCtx(Ctx ctx)
    {
        _ctx = ctx;
        _pausedPlayers = new List<bool>(new bool[videoPlayers.Count]);
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
            StopVideo(player);

        for (var i = 0; i < _pausedPlayers.Count; i++) 
            _pausedPlayers[i] = false;
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

        var texture = (RenderTexture) player.GetComponent<RawImage>().texture;
        texture.Release();
    }

    public void PauseVideoPlayer()
    {
        for (var i = 0; i < videoPlayers.Count; i++)
        {
            if (!videoPlayers[i].isPlaying) continue;
            videoPlayers[i].Pause();
            _pausedPlayers[i] = true;
        }
    }

    public void ResumeVideoPlayer()
    {
        for (var i = 0; i < videoPlayers.Count; i++)
        {
            if (!_pausedPlayers[i]) continue;
            videoPlayers[i].Play();
            _pausedPlayers[i] = false;
        }
    }
}