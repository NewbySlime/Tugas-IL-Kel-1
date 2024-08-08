using UnityEngine;


/// <summary>
/// Static class that contains utility functions for Unity's objects.
/// </summary>
public static class ObjectUtility{
  /// <summary>
  /// Check if an object has been initialized. This uses interface <see cref="IObjectInitialized"/> inside the target object.
  /// </summary>
  /// <param name="obj">The target object to check</param>
  /// <returns>Is the object ready or not yet</returns>
  public static bool IsObjectInitialized(GameObject obj){
    IObjectInitialized[] _interface_list = obj.GetComponents<IObjectInitialized>();
    foreach(IObjectInitialized _interface in _interface_list){
      if(!_interface.GetIsInitialized())
        return false;
    }

    return true;
  }

  /// <summary>
  /// <inheritdoc cref="IsObjectInitialized"/>
  /// This does not only check a single component, instead it will check the <b>GameObject</b> attached to the component.
  /// </summary>
  /// <param name="comp">The target component to check</param>
  /// <returns><inheritdoc cref="IsObjectInitialized"/></returns>
  public static bool IsObjectInitialized(Component comp){
    return IsObjectInitialized(comp.gameObject);
  }


  /// <summary>
  /// Get hierarchy path in string format of a target object. This will check recursively its parent.
  /// </summary>
  /// <param name="obj">The target object to check</param>
  /// <returns>The hierarchy path</returns>
  public static string GetObjHierarchyPath(Transform obj){
    if(obj == null)
      return "";
      
    return GetObjHierarchyPath(obj.parent) + " > " + obj.name;
  }
}