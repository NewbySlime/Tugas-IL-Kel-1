using UnityEngine;
using Unity.VisualScripting;
using System.Collections;


namespace SequenceHelper{
  public class TriggerSoundSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "trigger_sound";

    public struct SequenceData{
      public ObjectReference.ObjRefID SoundObjectRef;

      public AudioClip Audio;
      public float Volume;

      public float Delay;
    }


    private SequenceData _seq_data;


    private IEnumerator _trigger_sound(AudioSource source){
      float _timer = _seq_data.Delay;
      while(_timer > 0){
        yield return null;
        _timer -= Time.deltaTime;
      }

      if(!source.isPlaying)
        source.Play();
    }


    public void StartTriggerAsync(){
      GameObject _target_obj = ObjectReference.GetReferenceObject(_seq_data.SoundObjectRef);
      if(_target_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.SoundObjectRef));
        return;
      }

      AudioSource _audio_obj = _target_obj.GetComponent<AudioSource>();
      if(_audio_obj == null){
        Debug.LogError(string.Format("Referenced Object does not have AudioSource. (RefID: {0})", _seq_data.SoundObjectRef));
        return;
      }

      if(_seq_data.Audio != null)
        _audio_obj.clip = _seq_data.Audio;

      _audio_obj.volume = _seq_data.Volume;
      StartCoroutine(_trigger_sound(_audio_obj));
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


  [UnitTitle("Trigger Sound")]
  [UnitCategory("Sequence/Game/Sound")]
  public class TriggerSoundSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _sound_target_obj_input;

    [DoNotSerialize]
    private ValueInput _audio_clip_input;

    [DoNotSerialize]
    private ValueInput _volume_input;

    [DoNotSerialize]
    private ValueInput _delay_input;


    protected override void Definition(){
      base.Definition();

      _sound_target_obj_input = ValueInput<ObjectReference.ObjRefID>("SoundObjRef");
      _audio_clip_input = ValueInput<AudioClip>("Audio");
      _volume_input = ValueInput("Volume", 1f);
      _delay_input = ValueInput("Delay", 0f);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = TriggerSoundSequence.SequenceID,
        SequenceData = new TriggerSoundSequence.SequenceData{
          SoundObjectRef = flow.GetValue<ObjectReference.ObjRefID>(_sound_target_obj_input),
          Audio = _audio_clip_input.hasAnyConnection? flow.GetValue<AudioClip>(_audio_clip_input): null,
          Volume = flow.GetValue<float>(_volume_input),
          Delay = flow.GetValue<float>(_delay_input)
        }
      };
    }
  }
}