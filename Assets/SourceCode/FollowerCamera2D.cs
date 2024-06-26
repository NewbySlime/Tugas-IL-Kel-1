using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class FollowerCamera2D: MonoBehaviour{
  public static ObjectReference.ObjRefID DefaultRefID = new(){
    ID = "scene_camera_obj"
  };


  private struct _follower_data{
    public GameObject _target;
    
    public float weight;
  }
  
  [SerializeField] [Range(0,1)]
  private float _PlayerToMouseCameraRatio = 1f/3;

  public float DefaultSmoothTime = 0.1f;

  [SerializeField]
  private bool _BindToPlayerOnStart = true;


  private Dictionary<GameObject, _follower_data> _list_followed = new Dictionary<GameObject, _follower_data>();

  private Vector3 _last_pivot_position;
  private GameObject _pivot_object;

  private Vector3 _last_position;

  private GameHandler _game_handler;
  private GameTimeHandler _time_handler;

  private float _z_position = 0;

  private float _current_vel = 0;

  private float _smooth_time;


  private void _on_scene_changed(string scnee_id, GameHandler.GameContext context){
    ObjectReference.SetReferenceObject(DefaultRefID, gameObject);
  }

  private void _on_scene_removed(){
    _game_handler.SceneChangedFinishedEvent -= _on_scene_changed;
    _game_handler.SceneRemovingEvent -= _on_scene_removed;
  }


  public void Start(){
    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _on_scene_changed;
    _game_handler.SceneRemovingEvent += _on_scene_removed;

    _time_handler = FindAnyObjectByType<GameTimeHandler>();
    if(_time_handler == null)
      Debug.LogWarning("Cannot find GameHandler.");

    _z_position = transform.position.z;
    _last_position = transform.position;

    _smooth_time = DefaultSmoothTime;

    if(_BindToPlayerOnStart)
      RevertDefaultPivot();
  } 

  public void Update(){
    DEBUGModeUtils.Log(string.Format("follower camera pos {0}", transform.position));
    if(_time_handler != null && _time_handler.IsPausing())
      return;

    Vector3 _next_pos = _last_pivot_position;
    if(_pivot_object != null){
      _next_pos = _pivot_object.transform.position / (_list_followed.Count > 0? 2: 1);
      _last_pivot_position = _pivot_object.transform.position;
    }

    _next_pos.z = _z_position;

    if(_list_followed.Count > 0){
      Vector3 _target_pos = Vector3.zero;
      float _total_weight = 0;

      foreach(_follower_data _target_data in _list_followed.Values){
        _target_pos += _target_data._target.transform.position * _target_data.weight;
        _total_weight += _target_data.weight;
      }

      _target_pos /= _list_followed.Count * _total_weight;

      _target_pos.z = _z_position;
      _next_pos += _target_pos;
    }

    Vector3 _dir_to_next;
    float _distance_next;
    {Vector3 _delta_from_next = _next_pos-transform.position;
      _dir_to_next = _delta_from_next.normalized;
      _distance_next = _delta_from_next.magnitude;
    }

    float _distance_last;
    {Vector3 _delta_from_last = transform.position-_last_position;
      _distance_last = _delta_from_last.magnitude;
    }

    float _next_value = Mathf.SmoothDamp(_distance_last, _distance_next, ref _current_vel, _smooth_time);
    float _next_value_clamp = Mathf.Clamp(_next_value-_distance_last, 0, _distance_next);

    Vector3 _new_pos = transform.position + _dir_to_next * _next_value_clamp;
    _new_pos.z = _z_position;

    transform.position = _new_pos;
    _last_position = transform.position;
  }


  public void SetFollowObject(GameObject target, float weight){
    if(_list_followed.ContainsKey(target)){
      _follower_data _data = _list_followed[target];
      _data.weight = weight;
    }
    else{
      _list_followed[target] = new _follower_data{
        _target = target,
        weight = weight
      };
    }
  }

  public void RemoveFollowObject(GameObject target){
    if(!_list_followed.ContainsKey(target))
      return;

    _list_followed.Remove(target);
  }

  public void ClearFollowObject(){
    _list_followed.Clear();
  }


  #nullable enable
  public void SetPivotObject(GameObject? target = null){
    _pivot_object = target;
  }
  #nullable disable


  public void SetSmoothTime(float time){
    _smooth_time = time;
    _current_vel = 0;
  }


  public bool IsFollowingObject(GameObject target){
    return _list_followed.ContainsKey(target);
  }


  public void RevertDefaultPivot(){
    PlayerController _player = FindAnyObjectByType<PlayerController>();
    if(_player == null)
      return;

    MouseFollower _mouse_follower = FindAnyObjectByType<MouseFollower>();

    SetPivotObject(_player.gameObject);

    SetFollowObject(_player.gameObject, 1/_PlayerToMouseCameraRatio);
    SetFollowObject(_mouse_follower.gameObject, 1/(1-_PlayerToMouseCameraRatio));

    _smooth_time = DefaultSmoothTime;
  }
}