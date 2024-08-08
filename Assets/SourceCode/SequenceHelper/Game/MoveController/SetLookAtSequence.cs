using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system that force <see cref="MovementController"/> to look at a determined direction.
  /// </summary>
  public class SetLookAtSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "set_look_at";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target object that has <see cref="MovementController"/>.
      /// </summary>
      public ObjectReference.ObjRefID TargetControllerRef;

      /// <summary>
      /// The direction the <see cref="MovementController"/> has to look at.
      /// </summary>
      public Vector2 LookAt;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _target_obj = ObjectReference.GetReferenceObject(_seq_data.TargetControllerRef);
      if(_target_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.TargetControllerRef));
        return;
      }

      MovementController _target_controller = _target_obj.GetComponent<MovementController>();
      if(_target_controller == null){
        Debug.LogError(string.Format("Referenced Object does not have MovementController. (RefID: {0})", _seq_data.TargetControllerRef));
        return;
      }

      _target_controller.LookAt(_seq_data.LookAt);
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


  [UnitTitle("Set Look At")]
  [UnitCategory("Sequence/Game/MovementController")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="SetLookAtSequence"/>.
  /// </summary>
  public class SetLookAtSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _target_obj_ref_input;
    [DoNotSerialize]
    private ValueInput _look_at_input;


    protected override void Definition(){
      base.Definition();

      _target_obj_ref_input = ValueInput<ObjectReference.ObjRefID>("TargetController");
      _look_at_input = ValueInput("LookAt", Vector2.zero);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetLookAtSequence.SequenceID,
        SequenceData = new SetLookAtSequence.SequenceData{
          TargetControllerRef = flow.GetValue<ObjectReference.ObjRefID>(_target_obj_ref_input),
          LookAt = flow.GetValue<Vector2>(_look_at_input)
        }
      };
    }
  }
}