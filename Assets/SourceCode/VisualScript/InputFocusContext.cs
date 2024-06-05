using System.Collections.Generic;
using System.Data;
using UnityEditor.Rendering.LookDev;
using UnityEngine;


// use the same input focus, but layered
public class InputFocusContext: MonoBehaviour{
  public enum ContextEnum{
    Player,
    UI,
    Pause
  }

  // highest priority to lowest priority
  private static ContextEnum[] _ContextOrder = {ContextEnum.Pause, ContextEnum.UI, ContextEnum.Player};

  // end of list means have more rights
  private Dictionary<ContextEnum, List<int>> _bound_stack = new(){
    {ContextEnum.Player, new()},
    {ContextEnum.UI, new()},
    {ContextEnum.Pause, new()}
  };

  
  // if already available, it will push it to the top (end of list)
  public void RegisterInputObject(GameObject bind_obj, ContextEnum context){
    List<int> _stack = _bound_stack[context];
    int _obj_id = bind_obj.GetInstanceID();
    if(_stack.Contains(_obj_id))
      _stack.Remove(_obj_id);
    
    _stack.Add(_obj_id);
  }

  public void RegisterInputObject(Component bind_obj, ContextEnum context){
    RegisterInputObject(bind_obj.gameObject, context);
  }


  public void RemoveInputObject(GameObject bind_obj, ContextEnum context){
    List<int> _stack = _bound_stack[context];
    int _obj_id = bind_obj.GetInstanceID();
    if(!_stack.Contains(_obj_id))
      return;

    _stack.Remove(_obj_id);
  }

  public void RemoveInputObject(Component bind_obj, ContextEnum context){
    RemoveInputObject(bind_obj.gameObject, context);
  }


  public bool InputAvailable(GameObject bind_obj){
    foreach(ContextEnum _context in _ContextOrder){
      Debug.Log(string.Format("input context {0}", _context));
      List<int> _stack = _bound_stack[_context];
      Debug.Log(string.Format("input count {0}", _stack.Count));
      if(_stack.Count <= 0)
        continue;

      return _stack[_stack.Count-1] == bind_obj.GetInstanceID();
    }

    return false;
  }

  public bool InputAvailable(Component bind_obj){
    return InputAvailable(bind_obj.gameObject);
  }


  public void ClearRegisters(){
    foreach(List<int> _stack in _bound_stack.Values)
      _stack.Clear();
  }
}