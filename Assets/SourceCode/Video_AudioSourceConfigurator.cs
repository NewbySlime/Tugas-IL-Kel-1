using UnityEngine;
using UnityEngine.Video;


[RequireComponent(typeof(VideoPlayer))]
[RequireComponent(typeof(AudioSource))]
/// <summary>
/// Auto configurator component to instantly link Unity's <b>VideoPlayer</b> with <b>AudioSource</b>.
/// 
/// This class uses following component(s);
/// - Unity's <b>VideoPlayer</b>
/// - Unity's <b>AudioSource</b> 
/// </summary>
public class Video_AudioSourceConfigurator: MonoBehaviour{
  public void Awake(){
    VideoPlayer _player = GetComponent<VideoPlayer>();
    AudioSource _source = GetComponent<AudioSource>();
    
    _player.audioOutputMode = VideoAudioOutputMode.AudioSource;
    _player.SetTargetAudioSource(0, _source);
  }
}