using System.IO;
using UnityEngine;

namespace Data
{
    public class VideoPathBuilder
    {
        public string GetPath(string videoFileName) => 
            Path.Combine(Application.streamingAssetsPath, videoFileName);
    }
}