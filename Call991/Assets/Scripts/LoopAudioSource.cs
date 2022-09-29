using UnityEngine;

public class LoopAudioSource : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource = default;

    public void Play(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void Destroy()
    {
        audioSource.Stop();
        Destroy(gameObject);
    }
}