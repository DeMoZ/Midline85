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
    
    private Ctx _ctx;
    private string _currentVideoPath;
    private VideoClip _currentVideoClip;
}