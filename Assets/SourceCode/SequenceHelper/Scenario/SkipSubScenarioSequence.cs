using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  public class SkipSubScenarioSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "skip_sub_scenario";

    public struct SequenceData{
      public string ScenarioID;
    }

    
    private GameHandler _game_handler;

    private SequenceData _seq_data;


    public void Start(){
      _game_handler = FindAnyObjectByType<GameHandler>();
      if(_game_handler == null){
        Debug.LogError("Cannot get GameHandler.");
        throw new MissingReferenceException();
      }
    }


    public void StartTriggerAsync(){
      _game_handler._ScenarioDiagram.SkipToNextSubScenario(_seq_data.ScenarioID);
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


  [UnitTitle("Skip SubScenario")]
  [UnitCategory("Sequence/Scenario")]
  public class SkipSubScenarioSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _scenario_id_input;


    protected override void Definition(){
      base.Definition();

      _scenario_id_input = ValueInput("ScenarioID", "");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SkipSubScenarioSequence.SequenceID,
        SequenceData = new SkipSubScenarioSequence.SequenceData{
          ScenarioID = flow.GetValue<string>(_scenario_id_input)
        }
      };
    }
  }
}