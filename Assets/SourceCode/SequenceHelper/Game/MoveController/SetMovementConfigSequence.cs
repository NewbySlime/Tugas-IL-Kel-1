using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to set the configuration of <see cref="MovementController"/>.
  /// </summary>
  public class SetMovementConfigSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "set_movement_config";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target object to modify the config with.
      /// </summary>
      public ObjectReference.ObjRefID TargetObjRef;

      /// <summary>
      /// Flag if the <see cref="MovementController"/> should walk.
      /// </summary>
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
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="SetMovementConfigSequence"/>.
  /// </summary>
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