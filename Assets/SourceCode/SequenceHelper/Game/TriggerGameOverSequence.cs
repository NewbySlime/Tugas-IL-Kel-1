using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  /// <summary>
  /// A custom Sequencing system for triggering GameOver event by calling <see cref="GameHandler"/> function.
  /// </summary>
  public class TriggerGameOverSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    /// <summary>
    /// Sequence ID to be used for registering to <see cref="SequenceDatabase"/>.
    /// </summary>
    public const string SequenceID = "trigger_game_over";

    /// <summary>
    /// Data for the Sequence system.
    /// </summary>
    public struct SequenceData{
      /// <summary>
      /// The cause message to be used for the cause of GameOver.
      /// </summary>
      public string CauseText;
    }

    private GameHandler _game_handler;

    private SequenceData _seq_data;


    public void Start(){
      _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot find GameHandler.");
        throw new MissingReferenceException();
      }
    }


    public void StartTriggerAsync(){
      _game_handler.TriggerGameOver(_seq_data.CauseText);
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


  [UnitTitle("Trigger Game Over")]
  [UnitCategory("Sequence/Game")]
  /// <summary>
  /// An extended <see cref="AddSubSequence"/> node for sequence <see cref="TriggerGameOverSequence"/>.
  /// </summary>
  public class TriggerGameOverSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _cause_text_input;


    protected override void Definition(){
      base.Definition();

      _cause_text_input = ValueInput("CauseText", "");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = TriggerGameOverSequence.SequenceID,
        SequenceData = new TriggerGameOverSequence.SequenceData{
          CauseText = flow.GetValue<string>(_cause_text_input)
        }
      };
    }
  }
}