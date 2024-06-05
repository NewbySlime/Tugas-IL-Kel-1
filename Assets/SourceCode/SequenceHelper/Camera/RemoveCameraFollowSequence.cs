using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class RemoveCameraFollowSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "remove_camera_follow";
    
    public struct SequenceData{
      public ObjectReference.ObjRefID RefID;
    }


    private FollowerCamera2D _camera;

    private SequenceData _seq_data;


    public void Start(){
      _camera = FindAnyObjectByType<FollowerCamera2D>();
      if(_camera == null){
        Debug.LogWarning("Cannot find FollowerCamera2D.");
      }
    }


    public void StartTriggerAsync(){
      if(_camera == null) 
        return;

      GameObject _ref_obj = ObjectReference.GetReferenceObject(_seq_data.RefID);
      if(_ref_obj == null){
        Debug.LogError(string.Format("Referenced Object is null. (RefID: {0})", _seq_data.RefID));
        return;
      }

      _camera.RemoveFollowObject(_ref_obj);
    }

    public bool IsTriggering(){
      return false;
    }

    
    public string GetSequenceID(){
      return SequenceID;
    }

    public void SetSequenceData(object data){
      if(data is not SequenceData){
        Debug.LogError("Data is not SequenceData");
        return;
      }

      _seq_data = (SequenceData)data;
    }
  }


  [UnitTitle("Remove Camera Follow")]
  [UnitCategory("Sequence/Camera")]
  public class RemoveCameraFollowSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _ref_id_input;


    protected override void Definition(){
      base.Definition();

      _ref_id_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = RemoveCameraFollowSequence.SequenceID,
        SequenceData = new RemoveCameraFollowSequence.SequenceData{
          RefID = flow.GetValue<ObjectReference.ObjRefID>(_ref_id_input)
        }
      };
    }
  }
}