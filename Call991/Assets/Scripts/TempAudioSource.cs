using System;
using UnityEngine;

public class TempAudioSource : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource = default;

    private Action _onDestroyCallback;
    
    public void PlayAndDestroy(AudioClip audioClip, Action onDestroyCallback)
    {
        _onDestroyCallback = onDestroyCallback;
        audioSource.clip = audioClip;
        audioSource.loop = false;
        audioSource.Play();
        Invoke(nameof(Destroy), audioClip.length);
    }

    public void Pause(bool pause)
    {
        if (audioSource == null)
        {
            Destroy();
            return;
        }
        
        if (pause)
            audioSource.Pause();
        else
            audioSource.UnPause();
    }
    
    private void Destroy()
    {
        _onDestroyCallback?.Invoke();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Destroy();
    }
}