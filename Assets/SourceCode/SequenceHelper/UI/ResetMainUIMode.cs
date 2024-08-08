using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system reset the UI for Main.
  /// </summary>
  public class ResetMainUIMode: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "reset_main_ui_mode";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// Should hiding the UI, the animation would be skipped.
      /// </summary>
      public bool SkipAnimation;
    }


    private SequenceData _seq_data;

    private GameUIHandler _ui_handler;


    public void Start(){
      _ui_handler = FindAnyObjectByType<GameUIHandler>();
      if(_ui_handler == null){
        Debug.LogError("Cannot find GameUIHandler.");
        throw new MissingReferenceException();
      }
    }


    public void StartTriggerAsync(){
      _ui_handler.ResetMainUIMode(_seq_data.SkipAnimation);
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


  [UnitTitle("Reset Main UI Mode")]
  [UnitCategory("Sequence/UI")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="ResetMainUiMode"/>.
  /// </summary>
  public class ResetMainUIModeVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _skip_animation_input;


    protected override void Definition(){
      base.Definition();

      _skip_animation_input = ValueInput("SkipAnimation", false); 
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = ResetMainUIMode.SequenceID,
        SequenceData = new ResetMainUIMode.SequenceData{
          SkipAnimation = flow.GetValue<bool>(_skip_animation_input)
        }
      };
    }
  }
}