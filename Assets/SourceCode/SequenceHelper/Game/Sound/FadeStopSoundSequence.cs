using UnityEngine;
using Unity.VisualScripting;
using System.Collections;
using System.Data.Common;


namespace SequenceHelper{
  public class FadeStopSoundSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "fade_stop_sound";

    public struct SequenceData{
      public ObjectReference.ObjRefID SoundObject;

      public float FadeTime;
    }


    private SequenceData _seq_data;


    private IEnumerator _trigger_fade(AudioSource _source){
      float _source_volume = _source.volume;

      float _time = _seq_data.FadeTime;
      float _fade_timer = _seq_data.FadeTime;
      while(_fade_timer < 0){
        _source.volume = _fade_timer/_time * _source_volume;

        yield return null;
        _fade_timer -= Time.deltaTime;
      }

      _source.volume = 0;
      _source.Stop();
    }


    public void StartTriggerAsync(){
      GameObject _target_obj = ObjectReference.GetReferenceObject(_seq_data.SoundObject);
      if(_target_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.SoundObject));
        return;
      }

      AudioSource _target_source = _target_obj.GetComponent<AudioSource>();
      if(_target_source == null){
        Debug.LogError(string.Format("Referenced Object does not have AudioSource. (RefID: {0})", _seq_data.SoundObject));
        return;
      }

      StartCoroutine(_trigger_fade(_target_source));
    }

    public bool IsTriggering(){
      return false;
    }


    public string GetSequenceID(){
      return SequenceID;
    }

    public void SetSequenceData(object data){
      if(data is not SequenceData){
        Debug.LogError("Data is not SequenceData.");
        return;
      }

      _seq_data = (SequenceData)data;
    }
  }


  [UnitTitle("Fade Stop Sound")]
  [UnitCategory("Sequence/Game/Sound")]
  public class FadeStopSoundSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _sound_object_input;

    [DoNotSerialize]
    private ValueInput _fade_time_input;


    protected override void Definition(){
      base.Definition();

      _sound_object_input = ValueInput<ObjectReference.ObjRefID>("SoundObjectRef");
      _fade_time_input = ValueInput("FadeTime", 3f);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = FadeStopSoundSequence.SequenceID,
        SequenceData = new FadeStopSoundSequence.SequenceData{
          SoundObject = flow.GetValue<ObjectReference.ObjRefID>(_sound_object_input),
          FadeTime = flow.GetValue<float>(_fade_time_input)
        }
      };
    }
  }
}