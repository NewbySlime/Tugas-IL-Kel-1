using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Helper class for handling modifying certain components/objects related to help differentiate which are friendly or enemy to a player. For example, one of the component that will be modified by this is object's layer to make it only interact with damager from enemies.
/// 
/// This class modifies following object(s) for friendly context;
/// - Self <b>GameObject</b>'s layer.
/// 
/// This class uses external object(s);
/// - List of listener <b>GameObject</b>(s) to transmit the modifying message to.
///  
/// For any object to handle the friendly context on its own, catch message using function <b>ObjectFriendlyHandler_FriendlyTypeChanged(<see cref="FriendlyType"/>)</b>.
/// </summary>
public class ObjectFriendlyHandler: MonoBehaviour{
  /// <summary>
  /// Enum to differentiate which type of "friendly" (in player's perspective).
  /// </summary>
  public enum FriendlyType{
    PlayerFriend,
    Neutral,
    Enemy
  }

  // Stores layer mask needed for each friendly type.
  private static Dictionary<FriendlyType, LayerMask> _FriendlyContextLayerMaskMap;


  /// <summary>
  /// What type of "friendly" this object is (in player's perspective).
  /// </summary>
  public FriendlyType FriendlyContext;

  [SerializeField]
  private List<GameObject> _ListAffectedObject;


  private void _update_friendly_obj(GameObject obj, FriendlyType type){
    // object's layer
    obj.layer = _FriendlyContextLayerMaskMap[type];

    obj.SendMessage("ObjectFriendlyHandler_FriendlyTypeChanged", type, SendMessageOptions.DontRequireReceiver);
  }

  private IEnumerator _start_co_func(){
    yield return null;
    yield return new WaitForEndOfFrame();

    SetFriendlyType(FriendlyContext);
  }


  public void Start(){
    _FriendlyContextLayerMaskMap = new Dictionary<FriendlyType, LayerMask>{
      {FriendlyType.PlayerFriend, LayerMask.NameToLayer("Player")},
      {FriendlyType.Neutral, LayerMask.NameToLayer("Default")},
      {FriendlyType.Enemy, LayerMask.NameToLayer("Enemy")}
    };

    StartCoroutine(_start_co_func());
  }


  /// <summary>
  /// Sets the type of "friendly" of this object (in player's perspective).
  /// This function will modifies components/objects to readjust to new "friendly" type.
  /// </summary>
  /// <param name="type">What type of friendlies this object shoudl be</param>
  public void SetFriendlyType(FriendlyType type){
    _update_friendly_obj(gameObject, type);
    foreach(GameObject _obj in _ListAffectedObject)
      _update_friendly_obj(_obj, type);
  }
}