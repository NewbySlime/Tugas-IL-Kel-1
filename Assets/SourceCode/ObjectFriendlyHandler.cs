using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// note: use this not only self, 
public class ObjectFriendlyHandler: MonoBehaviour{
  public enum FriendlyType{
    PlayerFriend,
    Neutral,
    Enemy
  }

  private static Dictionary<FriendlyType, LayerMask> _FriendlyContextLayerMaskMap;


  public FriendlyType FriendlyContext;

  [SerializeField]
  private List<GameObject> _ListAffectedObject;


  private void _update_friendly_obj(GameObject obj, FriendlyType type){
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


  public void SetFriendlyType(FriendlyType type){
    _update_friendly_obj(gameObject, type);
    foreach(GameObject _obj in _ListAffectedObject)
      _update_friendly_obj(_obj, type);
  }
}