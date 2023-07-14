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
    private Coroutine[] _playRoutine;

    private void Start()
    {
        _playRoutine = new Coroutine[_playVideoAsLayers.Count];

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

        for (var i = 0; i < _playVideoAsLayers.Count; i++)
        {
            if (_playRoutine[i] != null)
                StopCoroutine(_playRoutine[i]);

            if (_playVideoAsLayers[i] == null) continue;

            _playRoutine[i] = StartCoroutine(PlayRoutine(_playVideoAsLayers[i], i));
        }
    }

    private IEnumerator PlayRoutine(VideoSet videoSet, int layer)
    {
        yield return new WaitForSeconds(videoSet.Delay);
        _videoManager.PlayVideo(videoSet, layer);
        _playRoutine[layer] = null;
    }


    [Button("Stop Videos")]
    private void StopVideos()
    {
        if (!_videoManager) return;

        foreach (var routine in _playRoutine)
        {
            if(routine != null)
                StopCoroutine(routine);
        }
        
        _videoManager.StopPlayers();
    }
}