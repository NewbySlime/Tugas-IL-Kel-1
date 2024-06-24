using UnityEngine;
using UnityEngine.Video;


[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(AudioSource))]
public class Video_AudioSourceConfigurator: MonoBehaviour{
  public void Awake(){
    VideoPlayer _player = GetComponent<VideoPlayer>();
    AudioSource _source = GetComponent<AudioSource>();
    
    _player.audioOutputMode = VideoAudioOutputMode.AudioSource;
    _player.SetTargetAudioSource(0, _source);
  }
}