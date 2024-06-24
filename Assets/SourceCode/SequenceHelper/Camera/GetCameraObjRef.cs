using UnityEngine;
using Unity.VisualScripting;


namespace SequenceHelper{
  [UnitTitle("Get Camera Obj")]
  [UnitCategory("Sequence/Camera")]
  public class GetCameraObjRef: Unit{
    [DoNotSerialize]
    private ValueOutput _camera_obj_ref_output;


    protected override void Definition(){
      _camera_obj_ref_output = ValueOutput("ObjRef", (flow) => {
        return FollowerCamera2D.DefaultRefID;
      });
    }
  }
}