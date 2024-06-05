using System;
using System.Threading;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class TimerSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public static string GetSequenceIDStatic(){ return "TimerSequence"; }

    [Serializable]
    public struct SeqData{
      public float Timer;
      public bool UseUnscaled;  
    }

    [SerializeField]
    private SeqData _data;

    private float _current_timer = 0;

    public void FixedUpdate(){
      if(_current_timer > 0)
        _current_timer -= _data.UseUnscaled?
          Time.fixedUnscaledDeltaTime:
          Time.fixedDeltaTime;
    }

    
    public void StartTriggerAsync(){
      _current_timer = _data.Timer;
    }

    public bool IsTriggering(){
      return _current_timer > 0;
    }


    public string GetSequenceID(){
      return GetSequenceIDStatic();
    }
    
    public void SetSequenceData(object data){
      if(data is not SeqData){
        Debug.LogWarning(string.Format("Data is not a '{0}'", typeof(SeqData).Name));
        return;
      }

      _data = (SeqData)data;
    }
  }

  [UnitTitle("Timer")]
  [UnitCategory("Sequence")]
  public class TimerSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _timer_value;
    [DoNotSerialize]
    private ValueInput _useunscaled_value;

    protected override void Definition(){
      base.Definition();

      _timer_value = ValueInput("Time", 0f);
      _useunscaled_value = ValueInput("UseUnscaled", false);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new SequenceHandlerVS.SequenceInitializeData.DataPart{
        SequenceID = TimerSequence.GetSequenceIDStatic(),
        SequenceData = new TimerSequence.SeqData{
          Timer = flow.GetValue<float>(_timer_value),
          UseUnscaled = flow.GetValue<bool>(_useunscaled_value)
        }
      };
    }
  }
}