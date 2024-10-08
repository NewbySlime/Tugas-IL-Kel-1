using UnityEngine;
using Pathfinding;
using System.Collections;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;


// TODO check stuck
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(MovementController))]
/// <summary>
/// Component for controlling <see cref="MovementController"/> autonomously using pathfinding in 2D space. For further explanation, see <b>Reference/Diagrams/AIMovement.drawio</b>
/// 
/// This class uses following component(s);
/// - Arongranberg's <b>Seeker</b> Pathfinding object for processing a graph of path to a target position.
/// - <see cref="MovementController"/> for movement used in the Game.
/// 
/// Thanks to Arongranberg's Pathfinding "A* Project" library to make this possible.
/// </summary>
public class PathFollower: MonoBehaviour{
  /// <summary>
  /// Event for when the follower has arrived to the target position.
  /// </summary>
  public event FinishedFollowing FinishedFollowingEvent;
  public delegate void FinishedFollowing();

  /// <summary>
  /// Event for when the follower stuck for some seconds
  /// </summary>
  public event StuckFollowing StuckFollowingEvent;
  public delegate void StuckFollowing();

  /// <summary>
  /// Event for when a path has been found.
  /// </summary>
  public event PathFound PathFoundEvent;
  public delegate void PathFound();
  /// <summary>
  /// Event for when the follower object cannot find a path.
  /// </summary>
  public event PathError PathErrorEvent;
  public delegate void PathError();


  [SerializeField]
  private float _StuckTimeout = 10;
  
  [SerializeField]
  private float _StuckSpeedThreshold = 10;

  [SerializeField]
  private float _PathUpdateInterval = 0.3f;

  
  [SerializeField]
  private float _PathStopDistance = 0.5f;
  [SerializeField]
  private float _PathSkipDistance = 1f;
  
  [SerializeField]
  private float _TopJumpAngleThreshold = 70;
  [SerializeField]
  private float _BottomJumpAngleThreshold = 30;

  [SerializeField]
  private float _ZigZagAngleThreshold = 0.3f;

  [SerializeField]
  private LayerMask _JumpCheckLayerMask;

  [SerializeField]
  private float _CheckDistanceFromGround = 0.75f;

  [SerializeField]
  private float _MaxTotalDistance = 10f;

  [SerializeField]
  private float _MaxTotalJumpDistance = 5f;

  [SerializeField]
  private bool _CanJump = true;


  private MovementController _movement;
  private Seeker _seeker;

  // blocking cancel
  private bool _jumping_flag = false;

  private Path _current_path;
  private bool _path_updated = false;

  private Vector3 _target_position;

  private Coroutine _move_coroutine = null;
  private Coroutine _update_coroutine = null;

  private bool _is_stuck = false;

  private bool _path_found = false;
  private bool _path_calculating = false;

  // Check if the path is valid to the follower conditions.
  private void _on_path_found(Path _new_path){
    DEBUGModeUtils.Log("Path found");
    if(!_new_path.error && _new_path.path.Count >= 1){
      DEBUGModeUtils.Log("Calculating path...");
      bool _success = true;

      float _total_distance = 0;
      Vector3 _last_node_pos = (Vector3)_new_path.path[0].position;
      for(int i = 1; i < _new_path.path.Count; i++){
        Vector3 _node_pos = (Vector3)_new_path.path[i].position;
        _total_distance += (_node_pos-_last_node_pos).magnitude;
        _last_node_pos = _node_pos;

        if(_total_distance > _MaxTotalDistance){
          _success = false;
          break;
        }
      }

      DEBUGModeUtils.Log("Path checked");
      _current_path = _new_path;
      _path_found = _success;
    }
    else{
      Debug.LogWarning(string.Format("Seeker cannot find suitable path. Seeker Error: {0}", _new_path.errorLog));
      _path_found = false;
    }

    _path_calculating = false;
  }

  private IEnumerator _find_new_path(Vector2 from_path, Vector2 target_path){
    _path_found = false;
    _path_calculating = true;
    _seeker.StartPath(from_path, target_path);

    yield return new WaitUntil(() => !_path_calculating);
  }

  
  // Update the path foreach an update interval, since the path is also based on this object's position
  private IEnumerator _update_path(){
    while(true){
      yield return new WaitForSeconds(_PathUpdateInterval);
      yield return _find_new_path(transform.position, _target_position);

      _path_updated = true;
    }
  }

  private Coroutine _current_trigger_coroutine = null;
  private IEnumerator _trigger_jump(Vector3 target_pos){
    _jumping_flag = true;
    yield return _movement.ForceJump(target_pos);
    _jumping_flag = false;

    yield return _find_new_path(transform.position, _target_position);
    _current_trigger_coroutine = null;
  }
  
  // Controls the movement based on the path to the target position.
  // See Reference/Diagrams/AIMovement.drawio for further explanation about the movement system.
  private IEnumerator _follow_path(Vector3 target_pos){
    _target_position = target_pos;
    DEBUGModeUtils.Log("finding new path...");
    yield return _find_new_path(transform.position, (Vector2)target_pos);

    if(!_path_found){
      Debug.LogWarning(string.Format("AI Stuck. (Pos: {0})", transform.position));
      _is_stuck = true;
      PathErrorEvent?.Invoke();

      CancelMoving();
      yield break;
    }

    DEBUGModeUtils.Log("Path found!");
    PathFoundEvent?.Invoke();
    if(_update_coroutine == null)
      _update_coroutine = StartCoroutine(_update_path());
    
    _is_stuck = false;
    int _current_path_idx = 0;
    while(true){
      if(_jumping_flag){
        yield return null;
        continue;
      }

      if(_path_updated){
        _path_updated = false;
        _current_path_idx = 0;
      }

      DEBUGModeUtils.Log("Check if stop...");
      GraphNode _current_node = null;
      int i = Mathf.Clamp(_current_path_idx, 0, _current_path.path.Count-1);

      // skip ke node selanjutnya
      Vector3 _previous_pos = transform.position;
        _previous_pos.z = 0;
      float _total_distance = 0;
      for(; i < _current_path.path.Count; i++){
        GraphNode _graph_node = _current_path.path[i];
        Vector3 _next_pos = (Vector3)_graph_node.position;
          _next_pos.z = 0;

        float _distance_to_next_path = (_next_pos-_previous_pos).magnitude;
        _total_distance += _distance_to_next_path;

        _previous_pos = (Vector3)_graph_node.position;
        

        DEBUGModeUtils.Log(string.Format("Distance {2} {0}/{1}", _total_distance, _PathStopDistance, i));

        if(i >= (_current_path.path.Count-1)){
          if(_total_distance < _PathStopDistance)
            continue; 
        }
        else if(_total_distance < _PathSkipDistance)
          continue;

        _current_node = _current_path.path[i];
        break;
      }

      // sudah selesai
      if(_current_node == null)
        break;

      DEBUGModeUtils.Log("Still not stopping.");
      _current_path_idx = i;

      Vector2 _general_direction = ((Vector3)_current_node.position-transform.position).normalized;

      bool _move_horizontally = true;
      RaycastHit2D _ray_hit = Physics2D.Raycast((Vector3)_current_node.position, Vector2.down, _CheckDistanceFromGround, _JumpCheckLayerMask);
      if(_ray_hit.collider == null || _ray_hit.collider.gameObject == gameObject){
        DEBUGModeUtils.Log("Check to jump...");
        bool _jumping_stuck = false;
        bool _skip_jump = false;
        GraphNode _ground_node = null;

        Vector3 _sum_direction = (Vector3)_current_node.position-transform.position;
        float _total_jump_distance = ((Vector3)_current_node.position-transform.position).magnitude;

        _previous_pos = (Vector3)_current_node.position;
        for(i += 1; i < _current_path.path.Count; i++){
          GraphNode _next_node = _current_path.path[i];

          Vector3 _delta_position = (Vector3)_next_node.position-_previous_pos;
          Vector3 _current_direction = _delta_position.normalized;
          _total_jump_distance += _delta_position.magnitude;
          if(_total_jump_distance >= _MaxTotalJumpDistance){
            _jumping_stuck = true;
            break;
          }

          _ray_hit = Physics2D.Raycast((Vector3)_next_node.position, Vector2.down, _CheckDistanceFromGround, _JumpCheckLayerMask);
          
          // jika "zig zag"
          // TODO kalau zig zag, path nya di set ke sebelah gameobject
          if(_ray_hit.collider != null &&
            ((_sum_direction.x > _ZigZagAngleThreshold && _current_direction.x < -_ZigZagAngleThreshold) ||
            (_sum_direction.x < -_ZigZagAngleThreshold && _current_direction.x > _ZigZagAngleThreshold))){
            _skip_jump = true;
            break;    
          }

          _sum_direction += (Vector3)_next_node.position-_previous_pos;
          DEBUGModeUtils.Log(string.Format("sum direction {0}", _sum_direction));
          _previous_pos = (Vector3)_next_node.position;

          if(_ray_hit.collider == null || _ray_hit.collider.gameObject == gameObject)
            continue;

          _ground_node = _next_node;
          break;
        }

        if(_jumping_stuck){
          _is_stuck = true;
          break;
        }

        _general_direction = _sum_direction.normalized;
        DEBUGModeUtils.Log(string.Format("new general dir {0}", _general_direction));

        if(_CanJump && (!_skip_jump || _general_direction.y <= 0)){
          if(_ground_node == null){
            _is_stuck = true;
            break;
          }

          float _y_angle = Mathf.Rad2Deg * Mathf.Asin(_general_direction.y);
          DEBUGModeUtils.Log(string.Format("check angle {0}", _y_angle));

          if(_y_angle >= _TopJumpAngleThreshold || _y_angle <= _BottomJumpAngleThreshold){
            yield return _trigger_jump((Vector3)_ground_node.position);

            _move_horizontally = false;
          }
        }
        
        if(!_CanJump)
          _skip_jump = true;
      }
      
      if(_move_horizontally){
        DEBUGModeUtils.Log("Not jumping, going to move horizontally...");
        float _speed = _movement.MovementSpeed;
        float _supposed_speed = _total_distance/Time.fixedDeltaTime;
        DEBUGModeUtils.Log(string.Format("speed {0}/{1}", _speed, _supposed_speed));

        float _val = 1;
        if(_speed > _supposed_speed)
          _val = _supposed_speed/_speed;

        if(_general_direction.x > 0)
          _movement.DoWalk(_val);
        else if(_general_direction.x < 0)
          _movement.DoWalk(-1*_val);
      }

      yield return new WaitForFixedUpdate();
    }
    
    CancelMoving();

    if(_is_stuck)
      StuckFollowingEvent?.Invoke();
    else
      FinishedFollowingEvent?.Invoke();
  }


  public void Start(){
    _movement = GetComponent<MovementController>();
    if(_movement == null){
      Debug.LogError("Cannot find Movement Controller.");
      return;
    }

    _seeker = GetComponent<Seeker>();
    if(_seeker == null){
      Debug.LogError("Cannot find AI Seeker.");
      return;
    }

    _seeker.pathCallback = _on_path_found;
  }


  /// <summary>
  /// Follow and move to a designated position.
  /// </summary>
  /// <param name="target_pos">The target position to move</param>
  public void FollowPathAsync(Vector3 target_pos){
    DEBUGModeUtils.Log(string.Format("is moving {0}", IsMoving()));
    if(IsMoving())
      return;

    _move_coroutine = StartCoroutine(_follow_path(target_pos));
  }

  /// <summary>
  /// Check if the follower object is currently stuck (no valid path is found).
  /// </summary>
  /// <returns>Is the follower object stuck</returns>
  public bool IsStuck(){
    return _is_stuck;
  }

  /// <summary>
  /// Check if the follower object is currently moving to a designated spot.
  /// </summary>
  /// <returns>Is the follower object moving</returns>
  public bool IsMoving(){
    return _move_coroutine != null;
  }

  /// <summary>
  /// Check if the follower object is currently jumping.
  /// </summary>
  /// <returns>Is the follower object jumping</returns>
  public bool IsJumping(){
    return _jumping_flag;
  }


  /// <summary>
  /// Prompt the follower object to cancel following a path to a designated spot.
  /// The cancellation process can be unsuccessful due to;
  /// - If the follower object is still jumping (<see cref="MovementController.ForceJump(Vector3)"/>).
  /// </summary>
  /// <returns>Is the cancellation successful or not</returns>
  public bool CancelMoving(){
    if(_jumping_flag)
      return false;

    if(_move_coroutine != null){
      StopCoroutine(_move_coroutine);
      _move_coroutine = null;
    }

    if(_update_coroutine != null){
      StopCoroutine(_update_coroutine);
      _update_coroutine = null;
    }

    _movement.DoWalk(0);
    return true;
  }


  /// <summary>
  /// Force this follower object to jump to a target position.
  /// This will ignore processings such as movement (following) processing. 
  /// </summary>
  /// <param name="target_jump">The target position</param>
  public void ForceJump(Vector3 target_jump){
    if(_current_trigger_coroutine != null)
      StopCoroutine(_current_trigger_coroutine);

    _current_trigger_coroutine = StartCoroutine(_trigger_jump(target_jump));
  }
}