using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class CheckUIRaycastBlock: MonoBehaviour{

  private string _get_obj_hierarchy(Transform obj){
    if(obj == null)
      return "";
      
    return _get_obj_hierarchy(obj.parent) + " > " + obj.name;
  }

  private void _trigger_mouse_raycast(){
    PointerEventData _current_mouse_event = new PointerEventData(EventSystem.current){
      position = Input.mousePosition
    };
    
    List<RaycastResult> _result_blocks = new();
    EventSystem.current.RaycastAll(_current_mouse_event, _result_blocks);

    Debug.Log(string.Format("Raycast Blocked: {0}", _result_blocks.Count <= 0? "null": _get_obj_hierarchy(_result_blocks[0].gameObject.transform)));
  }

  
  public void OnCheckTrigger(InputValue value){
    if(!value.isPressed)
      return;

    _trigger_mouse_raycast();
  }
}