using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip audioClip;
    public string soundPath;
    public string audioName;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayDialogue(string phraseName)
    {
        audioName = phraseName + ".wav";
        soundPath = "file://" + Application.streamingAssetsPath + "/Sounds/Dialogues/";
        StartCoroutine(LoadAudio());
    }

    private IEnumerator LoadAudio()
    {
        WWW request = GetAudioFromFile(soundPath, audioName);
        yield return request;

        audioClip = request.GetAudioClip();

        PlayAudioFile();
    }
    
    private void PlayAudioFile()
    {
        audioSource.clip = audioClip;
        audioSource.Play();
        audioSource.loop = false;
    }

    private WWW GetAudioFromFile(string path, string fileName)
    {
        string audioToLoad = string.Format(path + "{0}", fileName);
        WWW request = new WWW(audioToLoad);
        return request;
    }
}
