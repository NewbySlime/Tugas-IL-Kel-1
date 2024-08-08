using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Runtime.InteropServices.WindowsRuntime;
using SequenceHelper;




[UnitCategory("Scenario")]
/// <summary>
/// Visual Scripting Node for creating or adding new scenario data for use by <see cref="ScenarioDiagramVS"/>.
/// </summary>
public class AddScenario: Unit{
  [DoNotSerialize]
  private ControlInput _input_flow;
  [DoNotSerialize]
  private ControlOutput _output_flow;
  [DoNotSerialize]
  private ControlOutput _subscenario_flow;

  [DoNotSerialize]
  private ValueInput _scenario_input;
  [DoNotSerialize]
  private ValueOutput _scenario_output;

  [DoNotSerialize]
  private ValueInput _scenario_id;


  private ScenarioDiagramVS.ScenarioCollection _init_data;

  protected override void Definition(){
    _input_flow = ControlInput("InputFlow", (flow) => {
      if(!_scenario_input.hasAnyConnection)
        _init_data = new ScenarioDiagramVS.ScenarioCollection();
      else
        _init_data = flow.GetValue<ScenarioDiagramVS.ScenarioCollection>(_scenario_input);
      
      _init_data.ScenarioList.Add(new ScenarioDiagramVS.ScenarioData{
        ScenarioID = flow.GetValue<string>(_scenario_id)
      });

      flow.Invoke(_subscenario_flow);

      return _output_flow;
    });

    _output_flow = ControlOutput("OutputFlow");
    _subscenario_flow = ControlOutput("ScenarioFlow");

    _scenario_input = ValueInput<ScenarioDiagramVS.ScenarioCollection>("ScenarioCollection");
    _scenario_output = ValueOutput("ScenarioCollection", (flow) => { return _init_data; });


    _scenario_id = ValueInput("ScenarioID", GameHandler.DefaultScenarioID);
  }
}



[UnitCategory("Scenario")]
/// <summary>
/// Visual Scripting Node for adding subscenario data to a scenario data for use by <see cref="ScenarioHandlerVS"/>.
/// </summary>
public class AddSubScenario: Unit{
  [DoNotSerialize]
  private ControlInput _input_flow;
  [DoNotSerialize]
  private ControlOutput _output_flow;

  [DoNotSerialize]
  private ValueInput _scenario_input;
  [DoNotSerialize]
  private ValueOutput _scenario_output;

  [DoNotSerialize]
  private ValueInput _sub_id;

  [DoNotSerialize]
  private ValueInput _seqinit_start;
  [DoNotSerialize]
  private ValueInput _quest_input;
  [DoNotSerialize]
  private ValueInput _seqinit_finish;

  
  private ScenarioDiagramVS.ScenarioCollection _init_data;

  protected override void Definition(){
    _input_flow = ControlInput("InputFlow", (flow) => {
      _init_data = flow.GetValue<ScenarioDiagramVS.ScenarioCollection>(_scenario_input);

      ScenarioDiagramVS.ScenarioData _scenario_data = _init_data.ScenarioList[_init_data.ScenarioList.Count-1];
      _scenario_data.SubscenarioList.Add(new ScenarioDiagramVS.ScenarioData.SubData{
        SubID = flow.GetValue<string>(_sub_id),

        SequenceStartData = _seqinit_start.hasAnyConnection?
          flow.GetValue<SequenceHandlerVS.SequenceInitializeData>(_seqinit_start):
          null,

        QuestData = _quest_input.hasAnyConnection?
          flow.GetValue<QuestHandlerVS.InitQuestInfo>(_quest_input):
          null,

        SequenceFinishData = _seqinit_finish.hasAnyConnection?
          flow.GetValue<SequenceHandlerVS.SequenceInitializeData>(_seqinit_finish):
          null
      });

      return _output_flow;
    });

    _scenario_input = ValueInput<ScenarioDiagramVS.ScenarioCollection>("ScenarioCollection");
    _scenario_output = ValueOutput("ScenarioCollection", (flow) => { return _init_data; });

    _output_flow = ControlOutput("NextFlow");

    _sub_id = ValueInput("SubscenarioID", "");
    
    _seqinit_start = ValueInput<SequenceHandlerVS.SequenceInitializeData>("StartSequence");
    _quest_input = ValueInput<QuestHandlerVS.InitQuestInfo>("QuestData");
    _seqinit_finish = ValueInput<SequenceHandlerVS.SequenceInitializeData>("FinishSequence");
  }
}