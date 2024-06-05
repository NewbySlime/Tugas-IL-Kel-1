using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class SetEnableScenarioSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "set_enable_scenario";

    public struct SequenceData{
      public string ScenarioID;
      public bool Active;
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
      _game_handler._ScenarioDiagram.SetEnableScenario(_seq_data.ScenarioID, _seq_data.Active);
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



  [UnitTitle("Set Enable Scenario")]
  [UnitCategory("Sequence/Scenario")]
  public class SetEnableScenarioSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _scenario_id_input;
    [DoNotSerialize]
    private ValueInput _active_input;


    protected override void Definition(){
      base.Definition();

      _scenario_id_input = ValueInput("ScenarioID", "");
      _active_input = ValueInput("Active", true);
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new(){
        SequenceID = SetEnableScenarioSequence.SequenceID,
        SequenceData = new SetEnableScenarioSequence.SequenceData{
          ScenarioID = flow.GetValue<string>(_scenario_id_input),
          Active = flow.GetValue<bool>(_active_input)
        }
      };
    }
  }
}