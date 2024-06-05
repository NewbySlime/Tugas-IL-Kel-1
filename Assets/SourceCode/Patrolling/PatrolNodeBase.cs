using UnityEngine;
using Unity.VisualScripting;
using Unity.VisualScripting.InputSystem;



namespace PatrolActions{
  [UnitCategory("Patrol/Interface")]
  public class PatrolNodeBase: Unit{
    [DoNotSerialize]
    private ControlInput _input_flow;
    [DoNotSerialize]
    private ControlOutput _output_flow;
    
    [DoNotSerialize]
    private ValueInput _input_data;
    [DoNotSerialize]
    private ValueOutput _output_data;

    private PatrolBehaviour.InitData _init_data;


    protected override void Definition(){
      _input_flow = ControlInput("InputFlow", (flow) => {
        if(_input_data.hasAnyConnection)
          _init_data = flow.GetValue<PatrolBehaviour.InitData>(_input_data);
        else
          _init_data = new PatrolBehaviour.InitData();

        AddData(flow, out PatrolAction _new_action);
        if(_new_action != null)
          _init_data.PatrolSet.Add(_new_action);

        return _output_flow;
      });

      _output_flow = ControlOutput("OutputFlow");
      
      _input_data = ValueInput<PatrolBehaviour.InitData>("PatrolData");
      _output_data = ValueOutput<PatrolBehaviour.InitData>("PatrolData", (flow) => _init_data);
    }

    protected virtual void AddData(Flow flow, out PatrolAction action){
      action = null;
    }
  }
}