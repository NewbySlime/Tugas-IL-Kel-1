using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class TriggerGameOverSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "trigger_game_over";

    public struct SequenceData{
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