using UnityEngine;

public class TempAudioSource : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource = default;

    public void PlayAndDestroy(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.loop = false;
        audioSource.Play();
        Invoke(nameof(Destroy), audioClip.length);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}