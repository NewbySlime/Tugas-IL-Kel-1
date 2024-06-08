using UnityEngine;


public static class ObjectUtility{
  public static bool IsObjectInitialized(GameObject obj){
    IObjectInitialized[] _interface_list = obj.GetComponents<IObjectInitialized>();
    foreach(IObjectInitialized _interface in _interface_list){
      if(!_interface.GetIsInitialized())
        return false;
    }

    return true;
  }

  public static bool IsObjectInitialized(Component comp){
    return IsObjectInitialized(comp.gameObject);
  }
}