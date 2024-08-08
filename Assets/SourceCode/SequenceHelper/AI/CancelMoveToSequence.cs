using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system that cancels the target's AI Pathfollower to make it stops moving by itself. 
  /// </summary>
  public class CancelMoveToSequence: MonoBehaviour, ISequenceAsync,
  ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "cancel_move_to";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// Target Object Reference to stop the AI.
      /// </summary>
      public ObjectReference.ObjRefID TargetRefID;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _target_obj = ObjectReference.GetReferenceObject(_seq_data.TargetRefID);
      if(_target_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.TargetRefID));
        return;
      }

      PathFollower _path_follower = _target_obj.GetComponent<PathFollower>();
      if(_path_follower == null){
        Debug.LogError(string.Format("Object does not have PathFollower. ({0}, RefID: {1})", _target_obj.name, _seq_data.TargetRefID));
        return;
      }

      _path_follower.CancelMoving();
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


  [UnitTitle("Cancel Move To")]
  [UnitCategory("Sequence/AI")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="CancelMoveToSequence"/>.
  /// </summary>
  public class CancelMoveToSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _target_obj_ref_input;


    protected override void Definition(){
      base.Definition();

      _target_obj_ref_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = CancelMoveToSequence.SequenceID,
        SequenceData = new CancelMoveToSequence.SequenceData{
          TargetRefID = flow.GetValue<ObjectReference.ObjRefID>(_target_obj_ref_input)
        }
      };
    }
  }
}