using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Rendering;


namespace SequenceHelper{
  public class FlipObjectSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "flip_object";

    public struct SequenceData{
      public ObjectReference.ObjRefID RefID;
      public bool FlipX;
      public bool FlipY;
    }


    private SequenceData _seq_data;


    public void StartTriggerAsync(){
      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.RefID);
      if(_ref_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.RefID));
        return;
      }

      FlipInterface _flip_handler = _ref_obj.GetComponent<FlipInterface>();
      if(_flip_handler == null){
        Debug.LogError(string.Format("Referenced Object does not have FlipInterface. (RefID: {0})", _seq_data.RefID));
        return;
      }
      
      _flip_handler.SetFlippedX(_seq_data.FlipX);
      _flip_handler.SetFlippedY(_seq_data.FlipY);
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


  [UnitTitle("Flip Object")]
  [UnitCategory("Sequence/Object")]
  public class FlipObjectSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _ref_obj_input;

    [DoNotSerialize]
    private ValueInput _flip_x_input;
    [DoNotSerialize]
    private ValueInput _flip_y_input;


    protected override void Definition(){
      base.Definition();

      _ref_obj_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");

      _flip_x_input = ValueInput("FlipX", false);
      _flip_y_input = ValueInput("FlipY", false);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = FlipObjectSequence.SequenceID,
        SequenceData = new FlipObjectSequence.SequenceData{
          RefID = flow.GetValue<ObjectReference.ObjRefID>(_ref_obj_input),

          FlipX = flow.GetValue<bool>(_flip_x_input),
          FlipY = flow.GetValue<bool>(_flip_y_input)
        }
      };
    }
  }
}