using System.Collections.Generic;
using System.Data;
using UnityEngine;


/// <summary>
/// Helper class that can be used by any object to know if certain object has the privilege to use the input coming from player. Any object can register an object for "imaginary" privilege system.
/// This works by using context as the priority level, and stack data structure to know which object can use the input. If there are two or more object registered within the same priority level, the newest registered object has the privilege to use the input, while the others cannot use the input until the newest object are removed from the stack.
/// For further explanation about the priority level, see <see cref="ContextEnum"/>.
/// </summary>
public class InputFocusContext: MonoBehaviour{
  /// <summary>
  /// Event for when an object has been registered to this class.
  /// </summary>
  public event FocusContextRegistered FocusContextRegisteredEvent;
  public delegate void FocusContextRegistered();

  /// <summary>
  /// The priority level used for privilege system.
  /// Priority level from highest to lowest:
  /// - <see cref="ContextEnum.Pause"/>
  /// - <see cref="ContextEnum.UI"/>
  /// - <see cref="ContextEnum.Player"/>
  /// </summary>
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

  
  /// <summary>
  /// Register an object to the privilege system. This will set the object as the highest priority in the level.
  /// If the target object already registered in the same level, the function will push it to the top (highest priority).
  /// </summary>
  /// <param name="bind_obj">The object to bind</param>
  /// <param name="context">The priority level</param>
  public void RegisterInputObject(GameObject bind_obj, ContextEnum context){
    List<int> _stack = _bound_stack[context];
    int _obj_id = bind_obj.GetInstanceID();
    if(_stack.Contains(_obj_id))
      _stack.Remove(_obj_id);
    
    _stack.Add(_obj_id);

    FocusContextRegisteredEvent?.Invoke();
  }

  /// <inheritdoc cref="RegisterInputObject"/>
  public void RegisterInputObject(Component bind_obj, ContextEnum context){
    RegisterInputObject(bind_obj.gameObject, context);
  }


  /// <summary>
  /// Remove the target object to the privilege system.
  /// If the target object does not exist in the same level, the function will immediately returns.
  /// </summary>
  /// <param name="bind_obj">The object to bind</param>
  /// <param name="context">The priority level</param>
  public void RemoveInputObject(GameObject bind_obj, ContextEnum context){
    List<int> _stack = _bound_stack[context];
    int _obj_id = bind_obj.GetInstanceID();
    if(!_stack.Contains(_obj_id))
      return;

    _stack.Remove(_obj_id);
  }

  /// <inheritdoc cref="RemoveInputObject"/>
  public void RemoveInputObject(Component bind_obj, ContextEnum context){
    RemoveInputObject(bind_obj.gameObject, context);
  }


  /// <summary>
  /// To check if the target object has the privilege to use the input.
  /// </summary>
  /// <param name="bind_obj">The target object</param>
  /// <returns>Can the object use the input</returns>
  public bool InputAvailable(GameObject bind_obj){
    foreach(ContextEnum _context in _ContextOrder){
      List<int> _stack = _bound_stack[_context];
      if(_stack.Count <= 0)
        continue;

      return _stack[_stack.Count-1] == bind_obj.GetInstanceID();
    }

    return false;
  }

  /// <inheritdoc cref="InputAvailable"/>
  public bool InputAvailable(Component bind_obj){
    return InputAvailable(bind_obj.gameObject);
  }


  /// <summary>
  /// Clear all registered object. This will reset the class to its initial state.
  /// </summary>
  public void ClearRegisters(){
    foreach(List<int> _stack in _bound_stack.Values)
      _stack.Clear();
  }
}