using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEditor.SceneManagement;


[UnitTitle("Add Dialogue")]
[UnitCategory("Dialogue")]
public class AddDialogue: Unit{
  [DoNotSerialize]
  private ControlInput _input_flow;
  [DoNotSerialize]
  private ControlOutput _output_flow;

  [DoNotSerialize]
  private ValueInput _dialogue_input;
  [DoNotSerialize]
  private ValueOutput _dialogue_output;

  [DoNotSerialize]
  private ValueInput _message_input;
  [DoNotSerialize]
  private ValueInput _dialogue_highlight;
  [DoNotSerialize]
  private ValueInput _sequence_input;

  [DoNotSerialize]
  private ControlOutput _extended_data_flow;

  private DialogueUI.DialogueSequence _dialogue_sequence;

  protected override void Definition(){
    _input_flow = ControlInput("Input Flow", (flow) => {
      if(_dialogue_input.hasAnyConnection)
        _dialogue_sequence = flow.GetValue<DialogueUI.DialogueSequence>(_dialogue_input);
      else
        _dialogue_sequence = new DialogueUI.DialogueSequence();

      DialogueUI.DialogueData _dialogue_data = new();

      if(_dialogue_highlight.hasAnyConnection){
        List<string> _highlighted_character = flow.GetValue<List<string>>(_dialogue_highlight);
        foreach(string _h in _highlighted_character)
          _dialogue_data.CharactersTalking.Add(_h);
      }

      _dialogue_data.Dialogue = flow.GetValue<string>(_message_input);
      _dialogue_data.SequenceData = _sequence_input.hasAnyConnection?
        flow.GetValue<SequenceHandlerVS.SequenceInitializeData>(_sequence_input):
        null;

      _dialogue_sequence.Sequence.Add(_dialogue_data);

      flow.Invoke(_extended_data_flow);
      return _output_flow;
    });

    _dialogue_input = ValueInput<DialogueUI.DialogueSequence>("DialogueData");
    _message_input = ValueInput("Dialogue", "");
    _dialogue_highlight = ValueInput<List<string>>("HighlightCharacters");
    _sequence_input = ValueInput<SequenceHandlerVS.SequenceInitializeData>("SequenceData");

    _output_flow = ControlOutput("Output Flow");
    _dialogue_output = ValueOutput("DialogueData", (flow) => _dialogue_sequence);
    _extended_data_flow = ControlOutput("Extended Flow");
  }
}



[UnitTitle("Set Dialogue")]
[UnitCategory("Dialogue")]
public class SetDialogueToGameObject: Unit{
  [DoNotSerialize]
  private ControlInput _input_flow;
  [DoNotSerialize]
  private ControlOutput _output_flow;

  [DoNotSerialize]
  private ValueInput _dialogue_data_input;

  [DoNotSerialize]
  private ValueInput _game_object_input;

  protected override void Definition(){
    _input_flow = ControlInput("Input Flow", (flow) => {
      DialogueUI.DialogueSequence _data = flow.GetValue<DialogueUI.DialogueSequence>(_dialogue_data_input);

      GameObject obj = flow.GetValue<GameObject>(_game_object_input);
      obj.SendMessage("DialogueData_SetInitData", _data);

      return _output_flow;
    });

    _dialogue_data_input = ValueInput<DialogueUI.DialogueSequence>("DialogueData");
    _game_object_input = ValueInput<GameObject>("GameObject");

    _output_flow = ControlOutput("Output Flow");
  }
}


[UnitTitle("Add Extended Dialogue")]
[UnitCategory("Dialogue/.interface")]
public class AddExtendedDialogueData: Unit{
  [DoNotSerialize]
  private ControlInput _input_flow;
  [DoNotSerialize]
  private ControlOutput _output_flow;

  [DoNotSerialize]
  private ValueInput _extended_data_input;
  [DoNotSerialize]
  private ValueOutput _extended_data_output;

  private DialogueUI.DialogueSequence _dialogue_sequence;

  protected virtual void SetExtendedData(Flow flow, DialogueUI.DialogueData data){}

  protected override void Definition(){
    _input_flow = ControlInput("Input Flow", (flow) => {
      _dialogue_sequence = flow.GetValue<DialogueUI.DialogueSequence>(_extended_data_input);
      SetExtendedData(flow, _dialogue_sequence.Sequence[_dialogue_sequence.Sequence.Count-1]);

      return _output_flow;
    });

    _extended_data_input = ValueInput<DialogueUI.DialogueSequence>("DialogueData");

    _output_flow = ControlOutput("Output Flow");
    _extended_data_output = ValueOutput("DialogueData", (flow) => _dialogue_sequence);
  }
}