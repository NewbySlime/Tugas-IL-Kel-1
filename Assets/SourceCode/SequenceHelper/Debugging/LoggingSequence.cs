using System;
using System.Reflection;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class LoggingSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public static string GetSequenceIDStatic(){ return "LoggingSequence"; }

    public enum LogType{
      Normal,
      Warning,
      Error
    }

    [Serializable]
    public struct SeqData{
      public LogType MessageType;
      public string Message;
    }

    [SerializeField]
    private SeqData _data;
    

    public void StartTriggerAsync(){
      switch(_data.MessageType){
        case LogType.Normal:{
          Debug.Log(_data.Message);
        }break;

        case LogType.Warning:{
          Debug.LogWarning(_data.Message);
        }break;

        case LogType.Error:{
          Debug.LogError(_data.Message);
        }break;
      }
    }

    public bool IsTriggering(){
      return false;
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


  [UnitTitle("Debug Log")]
  [UnitCategory("Sequence")]
  public class LoggingSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _logtype_value;
    [DoNotSerialize]
    private ValueInput _logmessage_value;


    protected override void Definition(){
      base.Definition();

      _logtype_value = ValueInput("LogType", LoggingSequence.LogType.Normal);
      _logmessage_value = ValueInput("Message", "");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new SequenceHandlerVS.SequenceInitializeData.DataPart{
        SequenceID = LoggingSequence.GetSequenceIDStatic(),
        SequenceData = new LoggingSequence.SeqData{
          MessageType = flow.GetValue<LoggingSequence.LogType>(_logtype_value),
          Message = flow.GetValue<string>(_logmessage_value)
        }
      };
    }
  }
}