using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system that makes the target's AI move to follow a certain <see cref="ObjectReference.ObjRefID"/>.
  /// </summary>
  public class MoveObjectToObjRefSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "move_object_to_obj_ref";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// Target Object Reference to make it follow.
      /// </summary>
      public ObjectReference.ObjRefID TargetRefID;
      /// <summary>
      /// The target position (object) to follow. 
      /// </summary>
      public ObjectReference.ObjRefID PositionRefID;

      /// <summary>
      /// Flag to let the Sequence system blocks the Sequencing until the AI finished following.
      /// </summary>
      public bool WaitUntilOnPosition;
    }

    
    private SequenceData _seq_data;

    private bool _sequence_triggering = false;
  
    private IEnumerator _start_trigger(){
      GameObject _obj = ObjectReference.GetReferenceObject(_seq_data.TargetRefID);
      if(_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (Target, RefID: {0})", _seq_data.TargetRefID));
        yield break;
      }

      PathFollower _path_follower = _obj.GetComponent<PathFollower>();
      if(_path_follower == null){
        Debug.LogError(string.Format("Object does not have PathFollower. (Target, RefID: {0})", _seq_data.TargetRefID));
        yield break;
      }

      GameObject _position_obj = ObjectReference.GetReferenceObject(_seq_data.PositionRefID);
      if(_position_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (Position, RefID: {0})", _seq_data.PositionRefID));
        yield break;
      }

      _sequence_triggering = _seq_data.WaitUntilOnPosition;

      _path_follower.FollowPathAsync(_position_obj.transform.position);
      
      if(_seq_data.WaitUntilOnPosition)
        yield return new WaitUntil(() => !_path_follower.IsMoving());

      _sequence_triggering = false;
    }

    
    public void StartTriggerAsync(){
      StartCoroutine(_start_trigger());
    }

    public bool IsTriggering(){
      return _sequence_triggering;
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


  [UnitTitle("Move Object To ObjectRef")]
  [UnitCategory("Sequence/AI")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="MoveObjectToObjRefSequence"/>.
  /// </summary>
  public class MoveObjectToObjRefSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _pos_obj_input;
    [DoNotSerialize]
    private ValueInput _target_obj_input;

    [DoNotSerialize]
    private ValueInput _wait_until_input;


    protected override void Definition(){
      base.Definition();
      
      _pos_obj_input = ValueInput<ObjectReference.ObjRefID>("PositionObj");
      _target_obj_input = ValueInput<ObjectReference.ObjRefID>("TargetObj");
      _wait_until_input = ValueInput("WaitUntilPosition", true);
    }


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = MoveObjectToObjRefSequence.SequenceID,
        SequenceData = new MoveObjectToObjRefSequence.SequenceData{
          PositionRefID = flow.GetValue<ObjectReference.ObjRefID>(_pos_obj_input),
          TargetRefID = flow.GetValue<ObjectReference.ObjRefID>(_target_obj_input),
          WaitUntilOnPosition = flow.GetValue<bool>(_wait_until_input)
        }
      };
    }
  }
}