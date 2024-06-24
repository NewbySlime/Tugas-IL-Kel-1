using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SetMovementConfigSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_movement_config";

    public struct SequenceData{
      public ObjectReference.ObjRefID TargetObjRef;

      public bool Walk;
    }

    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _obj = ObjectReference.GetReferenceObject(_seq_data.TargetObjRef);
      if(_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.TargetObjRef));
        return;
      }

      MovementController _movement = _obj.GetComponent<MovementController>();
      if(_movement == null){
        Debug.LogError(string.Format("Referenced Object does not have MovementController. ({0}, RefID: {1})", _obj.name, _seq_data.TargetObjRef));
        return;
      }

      
      _movement.ToggleWalk = _seq_data.Walk;
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


  [UnitTitle("Set Config")]
  [UnitCategory("Sequence/Game/MoveController")]
  public class SetMovementConfigSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _target_obj_input;

    [DoNotSerialize]
    private ValueInput _toggle_walk_input;


    protected override void Definition(){
      base.Definition();

      _target_obj_input = ValueInput<ObjectReference.ObjRefID>("TargetObjRef");
      _toggle_walk_input = ValueInput("ToggleWalk", false);
    }


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetMovementConfigSequence.SequenceID,
        SequenceData = new SetMovementConfigSequence.SequenceData{
          TargetObjRef = flow.GetValue<ObjectReference.ObjRefID>(_target_obj_input),
          Walk = flow.GetValue<bool>(_toggle_walk_input)
        }
      };
    }
  }
}