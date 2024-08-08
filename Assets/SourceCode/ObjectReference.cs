using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Static class to store an object based on the reference ID that can be defined determinantly or randomly by this class.
/// This class is useful for Sequencing system that initialized when an object is initialized, therefore the Sequencing system does not know which objects to use in runtime. Which is why when a Sequencing system needs an object, it will use the reference ID.
/// </summary>
public static class ObjectReference{
  /// <summary>
  /// ID used for storing a reference of <b>GameObject</b> in <see cref="ObjectReference"/>.
  /// </summary>
  public struct ObjRefID{
    /// <summary>
    /// The ID.
    /// </summary>
    public string ID;

    /// <summary>
    /// Translate <see cref="ObjRefID"/> to a string.
    /// </summary>
    /// <returns>The resulting string</returns>
    public override string ToString(){
      return ID;
    }

    /// <summary>
    /// Translate <see cref="ObjRefID"/> to hashcode.
    /// </summary>
    /// <returns>The resulting hashcode</returns>
    public override int GetHashCode(){
      return ID.GetHashCode();
    }
  }

  /// <summary>
  /// Null reference ID.
  /// </summary>
  /// <value></value>
  public static ObjRefID Null = new ObjRefID{
    ID = ""
  };


  private static Dictionary<ObjRefID, GameObject> _random_instance = new();


  private static ObjRefID _create_random_id(){
    return new(){
      ID = Random.Range(int.MinValue, int.MaxValue).ToString()
    };
  }


  /// <summary>
  /// Create a random reference ID. Can also be used for immediately storing an object by not passing the 'obj' parameter to null.
  /// </summary>
  /// <param name="obj">The object to store</param>
  /// <returns>Resulting random reference</returns>
  public static ObjRefID CreateRandomReference(GameObject obj = null){
    ObjRefID _result = _create_random_id();
    while(_random_instance.ContainsKey(_result))
      _result = _create_random_id();

    _random_instance[_result] = obj;
    return _result;
  }

  /// <summary>
  /// Store an object based on a determined reference ID.
  /// </summary>
  /// <param name="id">The reference ID</param>
  /// <param name="obj">The object to store</param>
  public static void SetReferenceObject(ObjRefID id, GameObject obj){
    _random_instance[id] = obj;
  }

  /// <summary>
  /// Get an object based on the reference ID.
  /// </summary>
  /// <param name="id">The reference ID</param>
  /// <returns>The stored object</returns>
  public static GameObject GetReferenceObject(ObjRefID id){
    if(!_random_instance.ContainsKey(id))
      return null;

    return _random_instance[id];
  }


  /// <summary>
  /// Remove a stored object based on the reference.
  /// </summary>
  /// <param name="id">The reference ID</param>
  /// <returns>Is removed successfully</returns>
  public static bool RemoveReference(ObjRefID id){
    if(!_random_instance.ContainsKey(id))
      return false;

    _random_instance.Remove(id);
    return true;
  }


  /// <summary>
  /// Clear all stored object in this class.
  /// </summary>
  public static void ClearReference(){
    _random_instance.Clear();
  }
}