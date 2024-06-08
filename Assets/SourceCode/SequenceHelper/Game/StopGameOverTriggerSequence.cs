using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class StopGameOverTriggerSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "stop_game_over_trigger";

    public struct SequenceData{
      public ObjectReference.ObjRefID TargetRef;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _target_obj = ObjectReference.GetReferenceObject(_seq_data.TargetRef);
      if(_target_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.TargetRef));
        return;
      }

      
      OnDeadGameOverTrigger _dead_go_trigger = _target_obj.GetComponent<OnDeadGameOverTrigger>();
      if(_dead_go_trigger != null)
        _dead_go_trigger.CancelTrigger();
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


  [UnitTitle("Stop Game Over Triggers")]
  [UnitCategory("Sequence/Game")]
  public class StopGameOverTriggerSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _target_obj_ref_input;


    protected override void Definition(){
      base.Definition();

      _target_obj_ref_input = ValueInput<ObjectReference.ObjRefID>("TargetRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = StopGameOverTriggerSequence.SequenceID,
        SequenceData = new StopGameOverTriggerSequence.SequenceData{
          TargetRef = flow.GetValue<ObjectReference.ObjRefID>(_target_obj_ref_input)
        }
      };
    }
  }
}