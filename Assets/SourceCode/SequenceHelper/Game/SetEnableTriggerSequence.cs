using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SetEnableAreaTriggerSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_enable_trigger";

    public struct SequenceData{
      public bool Enable;
    }


    private GameHandler _game_handler;

    private SequenceData _seq_data;


    public void Start(){
      _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot find GameHandler.");
        throw new MissingReferenceException();
      }
    }


    public void StartTriggerAsync(){
      _game_handler.AreaTriggerEnable = _seq_data.Enable;
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


  [UnitTitle("Set Enable Area Trigger")]
  [UnitCategory("Sequence/Game")]
  public class SetEnableAreaTriggerSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _enable_input;


    protected override void Definition(){
      base.Definition();

      _enable_input = ValueInput("Enable", true);
    }


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetEnableAreaTriggerSequence.SequenceID,
        SequenceData = new SetEnableAreaTriggerSequence.SequenceData(){
          Enable = flow.GetValue<bool>(_enable_input)
        }
      };
    }
  }
}