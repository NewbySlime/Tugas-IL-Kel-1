using UnityEngine;
using Unity.VisualScripting;
using System.Data.Common;


namespace SequenceHelper{
  public class SetObjectPositionRefObjSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_object_position_ref_obj";

    public struct SequenceData{
      public ObjectReference.ObjRefID TargetRefID;
      public ObjectReference.ObjRefID PositionRefID;
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
      
      _target_obj.transform.position = _position_obj.transform.position;
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
  public class SetObjectPositionRefObjSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _target_ref_obj_input;
    [DoNotSerialize]
    private ValueInput _position_ref_obj_input;


    protected override void Definition(){
      base.Definition();

      _target_ref_obj_input = ValueInput<ObjectReference.ObjRefID>("Target ObjectRef");
      _position_ref_obj_input = ValueInput<ObjectReference.ObjRefID>("PositionRef");
    }


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetObjectPositionRefObjSequence.SequenceID,
        SequenceData = new SetObjectPositionRefObjSequence.SequenceData{
          TargetRefID = flow.GetValue<ObjectReference.ObjRefID>(_target_ref_obj_input),
          PositionRefID = flow.GetValue<ObjectReference.ObjRefID>(_position_ref_obj_input)
        }
      };
    }
  }
}