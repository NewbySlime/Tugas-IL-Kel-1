using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system to set the smooth time value of the <see cref="FollowerCamera2D"/>.
  /// </summary>
  public class SetCameraSmoothSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "set_camera_smooth";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// Smooth time to be supplied.
      /// </summary>
      public float SmoothTime;
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
      _camera.SetSmoothTime(_seq_data.SmoothTime);
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


  [UnitTitle("Set Camera Smooth")]
  [UnitCategory("Sequence/Camera")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="SetCameraSmoothSequence"/>.
  /// </summary>
  public class SetCameraSmoothSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _smooth_time_input;


    protected override void Definition(){
      base.Definition();

      _smooth_time_input = ValueInput("SmoothTime", 0.2);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetCameraSmoothSequence.SequenceID,
        SequenceData = new SetCameraSmoothSequence.SequenceData{
          SmoothTime = flow.GetValue<float>(_smooth_time_input)
        }
      };
    }
  }
}