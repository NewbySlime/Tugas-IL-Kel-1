using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  [UnitTitle("Get Player Input Focus")]
  [UnitCategory("Sequence/Player")]
  public class GetPlayerInputFocusSequenceVS: Unit{
    [DoNotSerialize]
    private ValueOutput _input_focus_output;


    protected override void Definition(){
      _input_focus_output = ValueOutput("FocusContext", (flow) => PlayerController.PlayerInputContext); 
    }
  }
}