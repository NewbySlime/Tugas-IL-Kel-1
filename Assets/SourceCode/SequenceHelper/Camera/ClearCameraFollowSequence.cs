using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Specialized;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system that clears the camera's following list.
  /// </summary>
  public class ClearCameraFollowSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "clear_camera_follow";

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

      _camera.ClearFollowObject();
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


  [UnitTitle("Clear Camera Follow")]
  [UnitCategory("Sequence/Camera")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="ClearCameraFollowSequence"/>.
  /// </summary>
  public class ClearCameraFollowSequenceVS: AddSubSequence{
    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = ClearCameraFollowSequence.SequenceID,
        SequenceData = new ClearCameraFollowSequence.SequenceData{}
      };
    }
  }
}