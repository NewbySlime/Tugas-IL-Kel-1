using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system that revert the camera settings like following list, smooth time, and pivot object to default.
  /// </summary>
  public class RevertCameraDefaultSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "revert_camera_default";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{}


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

      _camera.RevertDefaultPivot();
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


  [UnitTitle("Revert Camera To Default")]
  [UnitCategory("Sequence/Camera")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="RevertCameraDefaultSequence"/>.
  /// </summary>
  public class RevertCameraDefaultSequenceVS: AddSubSequence{
    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = RevertCameraDefaultSequence.SequenceID,
        SequenceData = new RevertCameraDefaultSequence.SequenceData{}
      };
    }
  }
}