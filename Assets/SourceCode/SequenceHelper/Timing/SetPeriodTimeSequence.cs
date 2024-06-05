using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class SetPeriodTimeSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_period_time";

    public struct SequenceData{
      public GameTimeHandler.GameTimePeriod TimePeriod;
    }

    private SequenceData _seq_data;
    
    private GameTimeHandler _time_handler;


    public void Start(){
      _time_handler = FindAnyObjectByType<GameTimeHandler>();
      if(_time_handler == null){
        Debug.LogError("Cannot find GameTimeHandler.");
        throw new MissingReferenceException();
      }
    }


    public void StartTriggerAsync(){
      _time_handler.SetTimePeriod(_seq_data.TimePeriod);
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



  [UnitTitle("Set Period Time")]
  [UnitCategory("Sequence/Timing")]
  public class SetPeriodTimeSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _period_time_input;


    protected override void Definition(){
      base.Definition();
      
      _period_time_input = ValueInput("PeriodTime", GameTimeHandler.GameTimePeriod.Daytime);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetPeriodTimeSequence.SequenceID,
        SequenceData = new SetPeriodTimeSequence.SequenceData{
          TimePeriod = flow.GetValue<GameTimeHandler.GameTimePeriod>(_period_time_input)
        }
      };
    }
  }
}