using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;


[UnitTitle("HighlightCharacter")]
[UnitCategory("Dialogue")]
/// <summary>
/// Node that can be used for creating or adding to a list of highlighted character as a data that can be used by <see cref="AddDialogue"/> Node.
/// </summary>
public class AddHightlightCharacter: Unit{
  [DoNotSerialize]
  private ValueInput _highlight_input;
  [DoNotSerialize]
  private ValueOutput _highlight_output;

  [DoNotSerialize]
  private ValueInput _highlighted_character;

  private List<string> _list_highlighted;


  protected override void Definition(){
    _highlight_output = ValueOutput("HighlightCharacters", (flow) => {
      if(_highlight_input.hasAnyConnection)
        _list_highlighted = flow.GetValue<List<string>>(_highlight_input);
      else
        _list_highlighted = new List<string>();

      _list_highlighted.Add(flow.GetValue<string>(_highlighted_character));
      return _list_highlighted;
    });

    _highlight_input = ValueInput<List<string>>("HighlightCharacters");
    _highlighted_character = ValueInput("Character", "");
  }
}