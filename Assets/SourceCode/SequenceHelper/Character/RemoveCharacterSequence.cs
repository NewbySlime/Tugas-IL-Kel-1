using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class RemoveCharacterSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "remove_character_obj";

    public struct SequenceData{
      public ObjectReference.ObjRefID RefID;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.RefID);
      if(_ref_obj == null){
        Debug.LogWarning(string.Format("Reference object is null. (RefID: {0})", _seq_data.RefID));
        return;
      }

      Destroy(_ref_obj);
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


  [UnitTitle("Remove Character")]
  [UnitCategory("Sequence/Character")]
  public class RemoveCharacterSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _ref_id_input;


    protected override void Definition(){
      base.Definition();

      _ref_id_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = RemoveCharacterSequence.SequenceID,
        SequenceData = new RemoveCharacterSequence.SequenceData{
          RefID = flow.GetValue<ObjectReference.ObjRefID>(_ref_id_input)
        }
      };
    }
  }
}