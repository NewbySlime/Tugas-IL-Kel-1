using UnityEngine;
using Unity.VisualScripting;
using System.Data.Common;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to set the object position based on the target <see cref="ObjectReference.ObjRefID"/> using its <b>Transform</b>.
  /// </summary>
  public class SetObjectPositionRefObjSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "set_object_position_ref_obj";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The target object to be moved.
      /// </summary>
      public ObjectReference.ObjRefID TargetRefID;

      /// <summary>
      /// The source object's <b>Transform</b> to be used.
      /// </summary>
      public ObjectReference.ObjRefID PositionRefID;

      /// <summary>
      /// Offset position to make adjustments.
      /// </summary>
      public Vector3 OffsetPos;
    }


    private SequenceData _seq_data;
    

    public void StartTriggerAsync(){
      GameObject _target_obj = ObjectReference.GetReferenceObject(_seq_data.TargetRefID);
      if(_target_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (TargetObj, Ref: {0})", _seq_data.TargetRefID));
        return;
      }

      GameObject _position_obj = ObjectReference.GetReferenceObject(_seq_data.PositionRefID);
      if(_position_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (Position, RefID: {0})", _seq_data.PositionRefID));
        return;
      }
      
      _target_obj.transform.position = _position_obj.transform.position + _seq_data.OffsetPos;
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


  [UnitTitle("Set Object Position (RefObj)")]
  [UnitCategory("Sequence/Object")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="SetObjectPositionRefObjSequence"/>.
  /// </summary>
  public class SetObjectPositionRefObjSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _target_ref_obj_input;
    [DoNotSerialize]
    private ValueInput _position_ref_obj_input;

    [DoNotSerialize]
    private ValueInput _offset_pos_input;


    protected override void Definition(){
      base.Definition();

      _target_ref_obj_input = ValueInput<ObjectReference.ObjRefID>("Target ObjectRef");
      _position_ref_obj_input = ValueInput<ObjectReference.ObjRefID>("PositionRef");
      
      _offset_pos_input = ValueInput("OffsetPos", Vector3.zero);
    }


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetObjectPositionRefObjSequence.SequenceID,
        SequenceData = new SetObjectPositionRefObjSequence.SequenceData{
          TargetRefID = flow.GetValue<ObjectReference.ObjRefID>(_target_ref_obj_input),
          PositionRefID = flow.GetValue<ObjectReference.ObjRefID>(_position_ref_obj_input),

          OffsetPos = flow.GetValue<Vector3>(_offset_pos_input)
        }
      };
    }
  }
}