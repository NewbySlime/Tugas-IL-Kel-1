using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  [UnitTitle("Get Object Reference")]
  [UnitCategory("Sequence/ObjectHandling")]
  /// <summary>
  /// Sequence helper for getting <see cref="ObjectReference.ObjRefID"/> based on the determined ID.
  /// </summary>
  public class GetObjectReferenceSequenceVS: Unit{
    [DoNotSerialize]
    private ValueInput _str_id_input;

    [DoNotSerialize]
    private ValueOutput _ref_obj_output;


    protected override void Definition(){
      _ref_obj_output = ValueOutput("ObjectRef", (flow) => {
        return new ObjectReference.ObjRefID{
          ID = flow.GetValue<string>(_str_id_input)
        };
      });

      _str_id_input = ValueInput("ID", "");
    }
  }
}