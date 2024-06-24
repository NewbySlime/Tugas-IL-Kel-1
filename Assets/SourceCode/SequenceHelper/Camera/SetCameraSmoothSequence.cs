using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class SetCameraSmoothSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_camera_smooth";

    public struct SequenceData{
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