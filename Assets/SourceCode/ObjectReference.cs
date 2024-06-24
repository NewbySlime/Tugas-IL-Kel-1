using System.Collections.Generic;
using UnityEngine;

public static class ObjectReference{
  public struct ObjRefID{
    public string ID;

    public override string ToString(){
      return ID;
    }

    public override int GetHashCode(){
      return ID.GetHashCode();
    }
  }

  public static ObjRefID Null = new ObjRefID{
    ID = ""
  };


  private static Dictionary<ObjRefID, GameObject> _random_instance = new();


  private static ObjRefID _create_random_id(){
    return new(){
      ID = Random.Range(int.MinValue, int.MaxValue).ToString()
    };
  }


  public static ObjRefID CreateRandomReference(GameObject obj = null){
    ObjRefID _result = _create_random_id();
    while(_random_instance.ContainsKey(_result))
      _result = _create_random_id();

    _random_instance[_result] = obj;
    return _result;
  }

  public static void SetReferenceObject(ObjRefID id, GameObject obj){
    _random_instance[id] = obj;
  }

  public static GameObject GetReferenceObject(ObjRefID id){
    if(!_random_instance.ContainsKey(id))
      return null;

    return _random_instance[id];
  }


  public static bool RemoveReference(ObjRefID id){
    if(!_random_instance.ContainsKey(id))
      return false;

    _random_instance.Remove(id);
    return true;
  }


  public static void ClearReference(){
    _random_instance.Clear();
  }
}