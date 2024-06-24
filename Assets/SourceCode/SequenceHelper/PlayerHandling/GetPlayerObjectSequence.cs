using Unity;
using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  [UnitTitle("Get Player Object")]
  [UnitCategory("Sequence/Player")]
  public class GetPlayerObjectSequenceVS: Unit{
    [DoNotSerialize]
    private ValueOutput _player_obj_output;

    protected override void Definition(){
      _player_obj_output = ValueOutput("ObjectRef", (flow) => PlayerController.DefaultRefID);
    }
  }
}