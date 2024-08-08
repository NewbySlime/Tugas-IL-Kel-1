using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system that add the target object to the follow list of the global <b>Camera2D</b>.
  /// </summary>
  public class SetCameraFollowSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "set_camera_follow";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// Target Object Reference to be added.
      /// </summary>
      public ObjectReference.ObjRefID RefID;

      /// <summary>
      /// The follow weight used by <see cref="FollowerCamera2D"/>.
      /// </summary>
      public float Weight;
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

      _camera.SetFollowObject(_ref_obj, _seq_data.Weight);
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


  [UnitTitle("Set Camera Follow")]
  [UnitCategory("Sequence/Camera")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="SetCameraFollowSequence"/>.
  /// </summary>
  public class SetCameraFollowSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _ref_id_input;

    [DoNotSerialize]
    private ValueInput _weight_input;


    protected override void Definition(){
      base.Definition();

      _ref_id_input = ValueInput<ObjectReference.ObjRefID>("ObjectRef");
      _weight_input = ValueInput("Weight", 1f);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetCameraFollowSequence.SequenceID,
        SequenceData = new SetCameraFollowSequence.SequenceData{
          RefID = flow.GetValue<ObjectReference.ObjRefID>(_ref_id_input),
          Weight = flow.GetValue<float>(_weight_input)
        }
      };
    }
  }
}