using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SetObjectActiveSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_object_active";

    public struct SequenceData{
      public ObjectReference.ObjRefID RefID;
      public bool Active;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.RefID);
      if(_ref_obj == null){
        Debug.LogError(string.Format("Reference Object is not found. (RefID: {0})", _seq_data.RefID));
        return;
      }

      _ref_obj.SetActive(_seq_data.Active);
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


  [UnitTitle("Set Object Active")]
  [UnitCategory("Sequence/Object")]
  public class SetObjectActiveSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _ref_id_input;
    [DoNotSerialize]
    private ValueInput _active_input;


    protected override void Definition(){
      base.Definition();

      _ref_id_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
      _active_input = ValueInput("SetActive", true);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetObjectActiveSequence.SequenceID,
        SequenceData = new SetObjectActiveSequence.SequenceData{
          RefID = flow.GetValue<ObjectReference.ObjRefID>(_ref_id_input),
          Active = flow.GetValue<bool>(_active_input)
        }
      };
    }
  }
}