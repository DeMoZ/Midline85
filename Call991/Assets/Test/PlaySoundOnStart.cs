using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlaySoundOnStart : MonoBehaviour
{
    public AudioClip _audioClip;
    public AudioClip _audioClip2;

    void Start()
    {
        SoundManager.Instance.PlaySound(_audioClip, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            SoundManager.Instance.PlaySound(_audioClip2, 1);
        }
    }
}
