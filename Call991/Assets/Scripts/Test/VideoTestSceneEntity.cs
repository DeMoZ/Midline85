using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class VideoTestSceneEntity : MonoBehaviour
{
    [SerializeField] private VideoManager videoManagerPrefab;
    [SerializeField] private RectTransform videoManagerParent;

    [Space] [SerializeField] private List<VideoSet> _playVideoAsLayers;
    private VideoManager _videoManager;
    private float _time;

    private void Start()
    {
        _videoManager = Instantiate(videoManagerPrefab, videoManagerParent);
        _videoManager.SetCtx(new VideoManager.Ctx
        {
        });

        PlayVideos();
    }

    [Button("Play Videos")]
    private void PlayVideos()
    {
        if (!_videoManager) return;
        _time = Time.time;

        StopAllCoroutines();

        foreach (var vSet in _playVideoAsLayers)
        {
            StartCoroutine(PlayRoutine(vSet));
        }
    }

    private IEnumerator PlayRoutine(VideoSet videoSet)
    {
        yield return new WaitForSeconds(videoSet.Delay);

        Debug.Log($"time <color=yellow>{Time.time - _time}</color>; " +
                  $"Play video <color=yellow>{(videoSet.Clip ? videoSet.Clip.name : AaGraphConstants.None)}</color>;" +
                  $" delay = {videoSet.Delay}; layer = {videoSet.Layer}");
        _videoManager.PlayVideo(videoSet);
    }


    [Button("Stop Videos")]
    private void StopVideos()
    {
        if (!_videoManager) return;

        _videoManager.StopPlayers();
    }
}