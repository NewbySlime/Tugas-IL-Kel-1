using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



/// <summary>
/// Component to extend <b>Camera2D</b> functionality to automatically or determinantly follow a target object or more smoothly.
/// Target follow objects can be multiple so the expected position will be in-between those objects. The expected position can be offset by using weighting for each follow object to create a ratio.
/// For further explanation, check diagram in <b>Reference/Diagrams/FollowerCamera2D.drawio</b>
/// 
/// This class uses external component(s);
/// - A <b>GameObject</b> as the pivot object. This is set manually.
/// - Target <b>GameObject</b>(s) as follower objects. This is set manually.
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for game events and such.
/// - <see cref="GameTimeHandler"/> to know if the Game is pausing.
/// </summary>
public class FollowerCamera2D: MonoBehaviour{
  /// <summary>
  /// The reference representation of the single scene instance of the object.
  /// </summary>
  public static ObjectReference.ObjRefID DefaultRefID = new(){
    ID = "scene_camera_obj"
  };


  private struct _follower_data{
    public GameObject _target;
    
    public float weight;
  }
  
  [SerializeField] [Range(0,1)]
  private float _PlayerToMouseCameraRatio = 1f/3;

  /// <summary>
  /// The default smooth time value.
  /// </summary>
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

  private float _total_weight = 0;


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
    // this function handles smoothing and following effect to the camera.

    DEBUGModeUtils.Log(string.Format("follower camera pos {0}", transform.position));
    if(_time_handler != null && _time_handler.IsPausing())
      return;

    Vector3 _pivot_pos = _last_pivot_position;
    Vector3 _next_pos = _last_pivot_position;
    if(_pivot_object != null){
      _pivot_pos = _pivot_object.transform.position;
      _last_pivot_position = _pivot_pos;
    }

    if(_list_followed.Count > 0){
      Vector3 _target_pos = Vector3.zero;

      foreach(_follower_data _target_data in _list_followed.Values){
        Vector3 _delta_pos = _target_data._target.transform.position - _pivot_pos;
        _target_pos += _delta_pos * (_target_data.weight/_total_weight);
      }

      _target_pos /= _list_followed.Count;
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


  /// <summary>
  /// Add another object to follow. This function will replace existing object configuration with a new one.
  /// </summary>
  /// <param name="target">The target object to follow</param>
  /// <param name="weight">Following weight</param>
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

    _total_weight = 0;
    foreach(_follower_data _data in _list_followed.Values)
      _total_weight += _data.weight;
  }

  /// <summary>
  /// Remove an object that this object follows based on the target object. Will be ignored if the target object does not exist or not followed yet.
  /// </summary>
  /// <param name="target">The target object to remove</param>
  public void RemoveFollowObject(GameObject target){
    if(!_list_followed.ContainsKey(target))
      return;

    _list_followed.Remove(target);
  }

  /// <summary>
  /// Clear all objects that followed by this object.
  /// </summary>
  public void ClearFollowObject(){
    _list_followed.Clear();
  }


  #nullable enable
  /// <summary>
  /// Set the pivot object of this follower component.
  /// The parameter for the target object can be set to null stops its following system. When calling this function, preferrably to call <see cref="ClearFollowObject"/> to complete it to full stop.
  /// </summary>
  /// <param name="target">The target object</param>
  public void SetPivotObject(GameObject? target = null){
    _pivot_object = target;
  }
  #nullable disable


  /// <summary>
  /// Set the smoothing time for the effect.
  /// </summary>
  /// <param name="time">The new smoothing time</param>
  public void SetSmoothTime(float time){
    _smooth_time = time;
    _current_vel = 0;
  }


  /// <summary>
  /// To check if this component is following the object (pivot not included to be checked).
  /// </summary>
  /// <param name="target">The target object to check</param>
  /// <returns>Is the object being followed</returns>
  public bool IsFollowingObject(GameObject target){
    return _list_followed.ContainsKey(target);
  }


  /// <summary>
  /// Function to revert all configurations about this component to the default.
  /// Automatically follows the player and another object(s) needed to be followed. 
  /// </summary>
  public void RevertDefaultPivot(){
    PlayerController _player = FindAnyObjectByType<PlayerController>();
    if(_player == null)
      return;

    MouseFollower _mouse_follower = FindAnyObjectByType<MouseFollower>();

    SetPivotObject(_player.gameObject);

    SetFollowObject(_player.gameObject, _PlayerToMouseCameraRatio);
    SetFollowObject(_mouse_follower.gameObject, 1-_PlayerToMouseCameraRatio);

    _smooth_time = DefaultSmoothTime;
  }
}