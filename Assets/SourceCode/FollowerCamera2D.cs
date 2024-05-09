using System.Collections.Generic;
using UnityEngine;



public class FollowerCamera2D: MonoBehaviour{
  private struct _follower_data{
    public GameObject _target;
    
    public float weight;
  }
  
  [SerializeField] [Range(0,1)]
  private float _PlayerToMouseCameraRatio = 1f/3;

  [SerializeField]
  private bool _BindToPlayerOnStart = true;


  private Dictionary<GameObject, _follower_data> _list_followed = new Dictionary<GameObject, _follower_data>();

  private GameObject _pivot_object;

  private float _z_position = 0;


  public void Start(){
    _z_position = transform.position.z;

    if(_BindToPlayerOnStart){
      PlayerController _player = FindAnyObjectByType<PlayerController>();
      MouseFollower _mouse_follower = FindAnyObjectByType<MouseFollower>();

      SetPivotObject(_player.gameObject);

      AddFollowObject(_player.gameObject, 1/_PlayerToMouseCameraRatio);
      AddFollowObject(_mouse_follower.gameObject, 1/(1-_PlayerToMouseCameraRatio));
    }
  } 

  public void Update(){
    if(_list_followed.Count > 0){
      Vector3 _pivot_target_pos = Vector3.zero;
      if(_pivot_object != null)
        _pivot_target_pos = _pivot_object.transform.position / 2;

      Vector3 _target_pos = Vector3.zero;
      float _total_weight = 0;

      foreach(_follower_data _target_data in _list_followed.Values){
        _target_pos += _target_data._target.transform.position * _target_data.weight;
        _total_weight += _target_data.weight;
      }

      _target_pos /= _list_followed.Count * _total_weight;

      _target_pos.z = _z_position;
      transform.position = _target_pos + _pivot_target_pos;
    }
  }


  public void AddFollowObject(GameObject target, float weight){
    _list_followed[target] = new _follower_data{
      _target = target,
      weight = weight
    };
  }

  public bool SetFollowWeight(GameObject target, float weight){
    if(!_list_followed.ContainsKey(target))
      return false;

    _follower_data _data = _list_followed[target];
    _data.weight = weight;
    return true;
  }

  public void RemoveFollowObject(GameObject target){
    if(!_list_followed.ContainsKey(target))
      return;

    _list_followed.Remove(target);
  }


  #nullable enable
  public void SetPivotObject(GameObject? target = null){
    _pivot_object = target;
  }
  #nullable disable


  public bool IsFollowingObject(GameObject target){
    return _list_followed.ContainsKey(target);
  }
}