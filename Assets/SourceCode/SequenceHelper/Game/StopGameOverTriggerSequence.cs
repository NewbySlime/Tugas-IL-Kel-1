using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to stop <see cref="OnDeadGameOverTrigger"/> trigger from invoking GameOver event to <see cref="GameHandler"/>. 
  /// </summary>
  public class StopGameOverTriggerSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "stop_game_over_trigger";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target object that has <see cref="OnDeadGameOverTrigger"/> trigger component.
      /// </summary>
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
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="StopGameOverTriggerSequence"/>.
  /// </summary>
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