using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  public class StartScenarioSequence: MonoBehaviour, ISequenceAsync, ISequenceData{
    public const string SequenceID = "start_scenario";
    
    public struct SequenceData{
      public string ScenarioID;
    }

    private ScenarioDiagramVS _scenario_diagram;
    private SequenceData _seq_data;


    public void Start(){
      _scenario_diagram = FindAnyObjectByType<ScenarioDiagramVS>();
      if(_scenario_diagram == null){
        Debug.LogError("Cannot get Scenario Diagram.");
        throw new MissingComponentException();
      }
    }


    public void StartTriggerAsync(){
      StartCoroutine(_scenario_diagram.StartScenario(_seq_data.ScenarioID));
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


  [UnitTitle("Start Scenario")]
  [UnitCategory("Sequence/Scenario")]
  public class StartScenarioSequenceVS: AddSubSequence{
    [DoNotSerialize]
    private ValueInput _scenario_id_input;

    protected override void Definition(){
      base.Definition();

      _scenario_id_input = ValueInput("ScenarioID", "");
    }

    protected override void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){
      init_data = new SequenceHandlerVS.SequenceInitializeData.DataPart{
        SequenceID = StartScenarioSequence.SequenceID,
        SequenceData = new StartScenarioSequence.SequenceData{
          ScenarioID = flow.GetValue<string>(_scenario_id_input)
        }
      };
    }
  }
}