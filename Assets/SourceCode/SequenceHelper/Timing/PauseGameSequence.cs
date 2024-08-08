using System;
using System.Threading;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;



namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system for pausing the Game's time.
  /// </summary>
  public class PauseGameSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "pause_game_sequence";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// Flag to do pausing the time or not.
      /// </summary>
      public bool IsPausing;
    }

    private GameTimeHandler _time_handler;

    private SequenceData _seq_data;


    public void Start(){
      _time_handler = FindAnyObjectByType<GameTimeHandler>();
      if(_time_handler == null){
        Debug.LogError("Game Handler cannot be fetch.");
        throw new MissingComponentException();
      }
    }


    public void StartTriggerAsync(){
      if(_seq_data.IsPausing)
        _time_handler.StopTime();
      else
        _time_handler.ResumeTime();
    }

    public bool IsTriggering(){
      return false;
    }


    public string GetSequenceID(){
      return SequenceID;
    }

    public void SetSequenceData(object data){
      if(data is not SequenceData){
        Debug.LogError("Data is not a SequenceData");
        return;
      }

      _seq_data = (SequenceData)data;
    }
  }


  [UnitTitle("Pause Game")]
  [UnitCategory("Sequence")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="PauseGameSequence"/>.
  /// </summary>
  public class PauseGameSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _is_pausing_input;

    protected override void Definition(){
      base.Definition();

      _is_pausing_input = ValueInput("IsPausing", true);
    }


    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new SequenceHandlerVS.SequenceInitializeData.DataPart{
        SequenceID = PauseGameSequence.SequenceID,
        SequenceData = new PauseGameSequence.SequenceData{
          IsPausing = flow.GetValue<bool>(_is_pausing_input)
        }
      };
    }
  }
}