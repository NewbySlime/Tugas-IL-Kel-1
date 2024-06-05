using UnityEngine;
using Unity.VisualScripting;
using System;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;



[UnitCategory("Sequence/SequenceUtility")]
public class AddSequence: Unit{
  [DoNotSerialize]
  private ControlInput _input_flow;

  [DoNotSerialize]
  private ControlOutput _output_flow;
  [DoNotSerialize]
  private ControlOutput _subsequence_flow;

  [DoNotSerialize]
  private ValueInput _seqinit_input;
  [DoNotSerialize]
  private ValueOutput _seqinit_output;

  private SequenceHandlerVS.SequenceInitializeData _init_data;

  protected override void Definition(){
    _input_flow = ControlInput("InputFlow", (flow) => {
      if(!_seqinit_input.hasValidConnection){
        _init_data = new SequenceHandlerVS.SequenceInitializeData{
          SequenceList = new List<List<SequenceHandlerVS.SequenceInitializeData.DataPart>>()
        };
      }
      else{
        _init_data = flow.GetValue<SequenceHandlerVS.SequenceInitializeData>(_seqinit_input);
      }

      _init_data.SequenceList.Add(new());

      flow.Invoke(_subsequence_flow);
      return _output_flow;
    });

    _output_flow = ControlOutput("NextFlow");
    _subsequence_flow = ControlOutput("SubsequenceFlow");

    _seqinit_input = ValueInput<SequenceHandlerVS.SequenceInitializeData>("SequenceData");

    _seqinit_output = ValueOutput("SequenceData", (flow) => {
      return _init_data;
    });
  }
}

[UnitTitle("Create Without IO Flow")]
[UnitCategory("Sequence/SequenceUtility")]
public class CreateWIOSequence: Unit{
  [DoNotSerialize]
  private ControlOutput _seqinit_flow;
  
  [DoNotSerialize]
  private ValueOutput _seqinit_result;
  [DoNotSerialize]
  private ValueOutput _seqinit_new;

  private SequenceHandlerVS.SequenceInitializeData _init_data;


  protected override void Definition(){
    _seqinit_result = ValueOutput("SequenceResult", (flow) => {
      _init_data = new SequenceHandlerVS.SequenceInitializeData{
        SequenceList = new List<List<SequenceHandlerVS.SequenceInitializeData.DataPart>>()
      };

      _init_data.SequenceList.Add(new());

      flow.Invoke(_seqinit_flow);
      return _init_data;
    });

    _seqinit_flow = ControlOutput("SequenceFlow");
    _seqinit_new = ValueOutput("SequenceData", (flow) => _init_data);
  }
}


[UnitCategory(".Interface")]
public class AddSubSequence: Unit{
  [DoNotSerialize]
  private ControlInput _input_flow;
  [DoNotSerialize]
  private ControlOutput _output_flow;

  [DoNotSerialize]
  private ValueInput _seqinit_input;
  [DoNotSerialize]
  private ValueOutput _seqinit_output;

  
  private SequenceHandlerVS.SequenceInitializeData _init_data;

  protected override void Definition(){
    _input_flow = ControlInput("InputFlow", (flow) => {
      _init_data = flow.GetValue<SequenceHandlerVS.SequenceInitializeData>(_seqinit_input);
      if(_init_data.SequenceList.Count > 0){
        SequenceHandlerVS.SequenceInitializeData.DataPart _new_subseq;
        AddData(flow, out _new_subseq);

        if(_new_subseq != null)
          _init_data.SequenceList[_init_data.SequenceList.Count-1].Add(_new_subseq);
      }

      return _output_flow;
    });

    _output_flow = ControlOutput("NextSubdata");

    _seqinit_input = ValueInput<SequenceHandlerVS.SequenceInitializeData>("SequenceData");
    _seqinit_output = ValueOutput("SequenceData", (flow) => {
      return _init_data;
    });
  }


  protected virtual void AddData(Flow flow, out SequenceHandlerVS.SequenceInitializeData.DataPart init_data){init_data = null;}
}