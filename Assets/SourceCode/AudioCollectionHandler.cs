using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(AudioSource))]
/// <summary>
/// Class to store each <b>AudioClip</b> with its respective ID that will be used later on by another class or Unity's animation system.
/// 
/// This class uses following component(s);
/// - Unity's <b>AudioSource</b> for playing <b>AudioClip</b> used in this class.
/// </summary>
public class AudioCollectionHandler: MonoBehaviour{
  [Serializable]
  /// <summary>
  /// Data about an audio with ID to differentiate each audio stored in <see cref="AudioCollectionHandler"/>.
  /// </summary>
  public struct AudioData{
    /// <summary>
    /// The ID for this audio.
    /// </summary>
    public string ID;

    /// <summary>
    /// The audio data.
    /// </summary>
    public AudioClip Audio;
  }

  [SerializeField]
  private List<AudioData> _AudioDataList;

  private Dictionary<string, AudioData> _audio_dict = new();


  private AudioSource _audio_source;


  public void Start(){
    _audio_source = GetComponent<AudioSource>();

    foreach(AudioData data in _AudioDataList)
      _audio_dict[data.ID] = data;
  }


  /// <summary>
  /// Trigger a sound stored in this class with a respective ID to tell which audio to play.
  /// This function can be used within Unity's animation system.
  /// </summary>
  /// <param name="ID">The audio ID to be played</param>
  public void TriggerSound(string ID){
    if(!_audio_dict.ContainsKey(ID)){
      Debug.LogWarning(string.Format("Cannot find AudioData with ID: '{0}'.", ID));
      return;
    }

    AudioData _data = _audio_dict[ID];

    _audio_source.PlayOneShot(_data.Audio);
  }
}