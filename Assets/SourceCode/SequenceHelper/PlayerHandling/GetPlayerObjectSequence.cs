using Unity;
using Unity.VisualScripting;
using UnityEngine;


namespace SequenceHelper{
  [UnitTitle("Get Player Object")]
  [UnitCategory("Sequence/Player")]
  /// <summary>
  /// Sequence helper for getting <see cref="PlayerController"/> object as an <see cref="ObjectReference.ObjRefID"/>.
  /// </summary>
  public class GetPlayerObjectSequenceVS: Unit{
    [DoNotSerialize]
    private ValueOutput _player_obj_output;

    protected override void Definition(){
      _player_obj_output = ValueOutput("ObjectRef", (flow) => PlayerController.DefaultRefID);
    }
  }
}