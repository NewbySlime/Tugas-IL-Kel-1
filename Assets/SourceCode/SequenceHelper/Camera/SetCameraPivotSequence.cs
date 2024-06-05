using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SetCameraPivotSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_camera_pivot";

    public struct SequenceData{
      public ObjectReference.ObjRefID RefID;
    }

    private FollowerCamera2D _camera;

    private SequenceData _seq_data;


    public void Start(){
      _camera = FindAnyObjectByType<FollowerCamera2D>();
      if(_camera == null){
        Debug.LogError("Cannot find FollowerCamera2D.");
        throw new MissingReferenceException();
      }
    }

    
    public void StartTriggerAsync(){
      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.RefID);
      _camera.SetPivotObject(_ref_obj);
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


  [UnitTitle("Set Camera Pivot")]
  [UnitCategory("Sequence/Camera")]
  public class SetCameraPivotSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _ref_id_input;


    protected override void Definition(){
      base.Definition();

      _ref_id_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
    }


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetCameraPivotSequence.SequenceID,
        SequenceData = new SetCameraPivotSequence.SequenceData{
          RefID = _ref_id_input.hasAnyConnection?
            flow.GetValue<ObjectReference.ObjRefID>(_ref_id_input):
            ObjectReference.Null
        }
      };
    }
  }
}