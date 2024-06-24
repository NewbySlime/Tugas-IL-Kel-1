using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Audio Source
    [SerializeField] AudioSource MusicSource;
    [SerializeField] AudioSource SFXSource;

    // Audio Clip
    public AudioClip background;
    public AudioClip attack;
    public AudioClip collect;
    public AudioClip dead;
    public AudioClip dialog;
    public AudioClip heal;
    public AudioClip hurt;
    public AudioClip jump;
    public AudioClip lose;
    public AudioClip punch;
    public AudioClip run;
    public AudioClip walk;
    public AudioClip laugh;

    // Volume Control
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.5f;

    private void Start()
    {
        if (MusicSource != null && background != null)
        {
            MusicSource.clip = background;
            MusicSource.volume = musicVolume;
            MusicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (SFXSource != null && clip != null)
        {
            SFXSource.volume = sfxVolume;
            SFXSource.PlayOneShot(clip);
        }
    }
}