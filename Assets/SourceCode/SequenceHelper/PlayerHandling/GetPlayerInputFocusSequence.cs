using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  [UnitTitle("Get Player Input Focus")]
  [UnitCategory("Sequence/Player")]
  /// <summary>
  /// Sequence helper for getting input focus context used by <see cref="PlayerController"/>.
  /// </summary>
  public class GetPlayerInputFocusSequenceVS: Unit{
    [DoNotSerialize]
    private ValueOutput _input_focus_output;


    protected override void Definition(){
      _input_focus_output = ValueOutput("FocusContext", (flow) => PlayerController.PlayerInputContext); 
    }
  }
}