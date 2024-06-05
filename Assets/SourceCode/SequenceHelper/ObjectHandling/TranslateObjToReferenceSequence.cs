using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  [UnitTitle("Translate Obj To Reference")]
  [UnitCategory("Sequence/ObjectHandling")]
  public class TranslateObjToReferenceSequenceVS: Unit{
    [DoNotSerialize]
    private ValueInput _obj_input;

    [DoNotSerialize]
    private ValueOutput _ref_obj_output;


    protected override void Definition(){
      _ref_obj_output = ValueOutput("ObjectRef", (flow) => {
        ObjectReference.ObjRefID _result = ObjectReference.Null;
        if(_obj_input.hasAnyConnection)
          _result = ObjectReference.CreateRandomReference(flow.GetValue<GameObject>(_obj_input));

        return _result;
      });

      _obj_input = ValueInput<GameObject>("Object");
    }
  }
}