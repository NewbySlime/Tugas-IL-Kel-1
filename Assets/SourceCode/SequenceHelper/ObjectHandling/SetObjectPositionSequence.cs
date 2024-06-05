using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class SetObjectPositionSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_object_position";

    public struct SequenceData{
      public ObjectReference.ObjRefID RefID;
      public Vector3 SetPosition;
    }

    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.RefID);
      if(_ref_obj == null){
        Debug.LogError(string.Format("Cannot get Object (Ref: {0})", _seq_data.RefID));
        return;
      }  

      _ref_obj.transform.position = _seq_data.SetPosition;
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


  [UnitTitle("Set Object Position")]
  [UnitCategory("Sequence/Object")]
  public class SetObjectPositionSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _ref_obj_input;
    [DoNotSerialize]
    private ValueInput _pos_input;


    protected override void Definition(){
      base.Definition();

      _ref_obj_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
      _pos_input = ValueInput("Position", Vector3.zero);
    }


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetObjectPositionSequence.SequenceID,
        SequenceData = new SetObjectPositionSequence.SequenceData{
          SetPosition = flow.GetValue<Vector3>(_pos_input),
          RefID = flow.GetValue<ObjectReference.ObjRefID>(_ref_obj_input)
        }
      };
    }
  }
}