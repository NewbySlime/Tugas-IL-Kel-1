using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(AudioSource))]
public class AudioCollectionHandler: MonoBehaviour{
  [Serializable]
  public struct AudioData{
    public string ID;
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


  public void TriggerSound(string ID){
    if(!_audio_dict.ContainsKey(ID)){
      Debug.LogWarning(string.Format("Cannot find AudioData with ID: '{0}'.", ID));
      return;
    }

    AudioData _data = _audio_dict[ID];

    _audio_source.PlayOneShot(_data.Audio);
  }
}