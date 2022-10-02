using Configs;
using UnityEngine;
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
    
    private Ctx _ctx;
    private string _currentVideoPath;
    private VideoClip _currentVideoClip;

    public void SetCtx(Ctx ctx)
    {
        
    }

    public void PlayVideo(string sceneVideoUrl)
    {
        videoPlayer.url = sceneVideoUrl;
    }
}