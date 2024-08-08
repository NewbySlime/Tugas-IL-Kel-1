using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


/// <summary>
/// Testing class to know which UI Object that blocks a mouse input.
/// 
/// Reference: https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/
/// </summary>
public class CheckUIRaycastBlock: MonoBehaviour{
  private void _trigger_mouse_raycast(){
    PointerEventData _current_mouse_event = new PointerEventData(EventSystem.current){
      position = Input.mousePosition
    };
    
    List<RaycastResult> _result_blocks = new();
    EventSystem.current.RaycastAll(_current_mouse_event, _result_blocks);

    DEBUGModeUtils.Log(string.Format("Raycast Blocked: {0}", _result_blocks.Count <= 0? "null": ObjectUtility.GetObjHierarchyPath(_result_blocks[0].gameObject.transform)));
  }

  
  /// <summary>
  /// Input event catch function to trigger a check.
  /// </summary>
  /// <param name="value">Input data from Unity</param>
  public void OnCheckTrigger(InputValue value){
    if(!value.isPressed)
      return;

    _trigger_mouse_raycast();
  }
}