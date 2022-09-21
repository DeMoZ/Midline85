using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;


    public AudioSource[] _sources;
    /*public AudioSource _source,
                                        _musicSource2,
                                         _buttonSource,
                                         _bgSource,
                                         _effectSource,
                                         _effectSource2,
                                         _phraseSource,
                                         _phraseSource2,
                                         _phraseSource3,
                                         _phraseSource4;*/
                                        


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //_source = gameObject.AddComponent<AudioSource>();
        //_musicSource2 = gameObject.AddComponent<AudioSource>();
        //_buttonSource = gameObject.AddComponent<AudioSource>();
        //_bgSource = gameObject.AddComponent<AudioSource>();
        //_effectSource = gameObject.AddComponent<AudioSource>();
        //_effectSource2 = gameObject.AddComponent<AudioSource>();
        //_phraseSource = gameObject.AddComponent<AudioSource>();
        //_phraseSource2 = gameObject.AddComponent<AudioSource>();
        //_phraseSource3 = gameObject.AddComponent<AudioSource>();
        //_phraseSource4 = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip, int sourceNumber)
    {
        _sources[sourceNumber].clip = clip;
        _sources[sourceNumber].Play();
    }

    public void PauseSound(AudioSource source)
    {

    }
}
