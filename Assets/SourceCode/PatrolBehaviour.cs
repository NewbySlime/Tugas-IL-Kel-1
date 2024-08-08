using PatrolActions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[RequireComponent(typeof(PathFollower))]
[RequireComponent(typeof(MovementController))]
/// <summary>
/// Behaviour class for patrolling system. The system uses <see cref="PathFollower"/> as its autonomous movement and a set of actions that a patrolling object can do. The sets of action nodes contained in namespace <see cref="PatrolActions"/>.
/// This class also trigger Game Over event when a Game Over trigger object is registered. When an object has been registered to the trigger, there is a delay before a Game Over actually trigger.
/// For creating custom patrol action, use interface <see cref="PatrolAction"/>.
/// 
/// This uses Visual Scripting but seperate from sequencing system. For setting the patrolling data, use <see cref="PatrolBehaviour.SetInitData(PatrolBehaviour.InitData)"/>.
/// 
/// This class uses following component(s);
/// - <see cref="PathFollower"/> component for pathfinding and AI movement.
/// - <see cref="MovementController"/> for controlling movement system used in the Game.
/// 
/// This class uses external component(s);
/// - <see cref="AlertTrigger"/> for watching potential "enemy" (in this case, Player) object to be registered as a Game Over trigger object.
/// - <see cref="BaseProgressUI"/> Progress bar for "alerted" progress.
/// - <see cref="SoundAlertReceiver"/> receiver for "imaginary sound" transmitted by <see cref="SoundAlertTransceiver"/>. This object used for diverting patrol object's attantion.
/// </summary>
public class PatrolBehaviour: MonoBehaviour{
  [SerializeField]
  private float _TemporaryStopDelay = 2f;

  [SerializeField]
  private float _GameOverTimeout = 5f;

  [SerializeField]
  private AlertTrigger _AlertingObject;

  [SerializeField]
  private float __PathUpdateInterval = 0.3f;
  /// <summary>
  /// Update interval to recheck the pathfinding.
  /// </summary>
  internal float _PathUpdateInterval{get => __PathUpdateInterval;}

  [SerializeField]
  private BaseProgressUI _AlertProgress;

  [SerializeField]
  private SoundAlertReceiver _SoundReceiver = null;


  /// <summary>
  /// Data for <see cref="PatrolBehaviour"/> used as patrol actions.
  /// </summary>k
  public class InitData{
    /// <summary>
    /// Sets of patrolling actions.
    /// </summary>
    public List<PatrolAction> PatrolSet = new List<PatrolAction>();
  }


  // What type of actions to skip after an interruption.
  private static HashSet<Type> _allowed_skip_type = new HashSet<Type>{
    typeof(PatrolActions.WaitForSeconds),
    typeof(PatrolActions.LookAtDirection)
  };

  private GameHandler _game_handler;

  private PathFollower _path_follower;
  /// <summary>
  /// Autonomous movement used in this class.
  /// </summary>
  internal PathFollower _PathFollower{get => _path_follower;}

  private MovementController _movement_controller;
  /// <summary>
  /// Movement system used in this class.
  /// </summary>
  internal MovementController _MovementController{get => _movement_controller;}

  private InitData _init_data;

  private int _current_patrol_idx = 0;
  private Coroutine _patrol_coroutine = null;
  private Coroutine _patrol_tmpstop = null;
  private Coroutine _patrol_forcestopper = null;

  private float _stop_delay = 0f;
  private float _gameover_delay = 0f;

  private HashSet<GameObject> _gameover_triggers = new HashSet<GameObject>();


  // Coroutine function to play a sets of patrol actions repeatedly.
  private IEnumerator _patrolling_function(){
    if(_init_data == null || _init_data.PatrolSet.Count <= 0){
      Debug.LogWarning("Cannot continue patrol, InitData is not set or InitData Patrol list is empty.");
      yield break;
    }

    yield return null;
    yield return new WaitForEndOfFrame();

    int i = 0;
    for(; i < _init_data.PatrolSet.Count; i++){
      if(_current_patrol_idx >= _init_data.PatrolSet.Count)
        _current_patrol_idx = 0;
      
      PatrolAction _current_patrol = _init_data.PatrolSet[_current_patrol_idx];
      Type _patrol_type = _current_patrol.GetPatrolType();
      if(!_allowed_skip_type.Contains(_patrol_type))
        break;

      _current_patrol_idx++;
    }

    while(true){
      if(_current_patrol_idx >= _init_data.PatrolSet.Count)
        _current_patrol_idx = 0;

      PatrolAction _current_patrol = _init_data.PatrolSet[_current_patrol_idx];
      yield return _current_patrol.DoAction(this);

      _current_patrol_idx++;
    }
  }

  // Coroutine for forcing patrolling system to stop by delay.
  private IEnumerator _patrol_stopdelay(){
    if(_patrol_forcestopper == null)
      _patrol_forcestopper = StartCoroutine(_patrol_stopforce());

    while(_stop_delay > 0){
      yield return new WaitForFixedUpdate();
      _stop_delay -= Time.fixedDeltaTime;
    }

    if(_patrol_forcestopper != null)
      StopCoroutine(_patrol_forcestopper);
    _patrol_forcestopper = null;

    StartPatrol();
    _patrol_tmpstop = null;
  }

  // Coroutine for focing patrolling system to stop permanently.
  private IEnumerator _patrol_stopforce(){
    yield return new WaitUntil(StopPatrol);
    _patrol_forcestopper = null;
  }


  private void _patrol_soundalerted(GameObject source){
    _stop_delay = _TemporaryStopDelay;
    if(_patrol_tmpstop == null)
      StartCoroutine(_patrol_stopdelay());

    _movement_controller.LookAt((source.transform.position - transform.position).normalized);

    StartCoroutine(UIUtility.SetHideUI(_AlertingObject.gameObject, true, true));
  }


  // Restart the patrol action system.
  private void _restart_func(){
    StartPatrol();
  }


  private void _on_scene_changed(string scene_id, GameHandler.GameContext context){
    if(!gameObject.activeInHierarchy)
      return;

    _restart_func();
  }

  private void _on_scene_removing(){
    _game_handler.SceneChangedFinishedEvent -= _on_scene_changed;
    _game_handler.SceneRemovingEvent -= _on_scene_removing;
  }


  private void _on_alerted_enter(GameObject _object){
    // check if player has entered, then adds it to trigger for game over
    {PlayerController _player = _object.GetComponent<PlayerController>();
      if(_player != null){
        DEBUGModeUtils.Log("Player entered");
        if(_gameover_triggers.Count <= 0){
          DEBUGModeUtils.Log("Player first entered");
          if(_gameover_delay < 0 || _gameover_delay > _GameOverTimeout)
            _gameover_delay = _GameOverTimeout;

          _stop_delay = _TemporaryStopDelay;

          DEBUGModeUtils.Log("trigger alert show");
          if(_AlertProgress != null)
            StartCoroutine(UIUtility.SetHideUI(_AlertProgress.gameObject, false));

          if(_patrol_tmpstop == null)
            _patrol_tmpstop = StartCoroutine(_patrol_stopdelay());
        }

        DEBUGModeUtils.Log("gameover add");
        _gameover_triggers.Add(_object);
      }
    }
  }

  private void _on_alerted_exit(GameObject _object){
    // check if player has exited, then removes it from trigger for game over
    {PlayerController _player = _object.GetComponent<PlayerController>();
      if(_player != null){
        _gameover_triggers.Remove(_object);
      }
    }
  }


  private void _update_alert_bar(){
    if(_AlertProgress == null)
      return;

    _AlertProgress.SetProgress(1-(_gameover_delay/_GameOverTimeout));
  }


  ~PatrolBehaviour(){
    _on_scene_removing();
  }


  private IEnumerator _start_co_func(){
    yield return new WaitUntil(() => ObjectUtility.IsObjectInitialized(_AlertingObject));

    if(_AlertProgress == null)
      yield break;

    StartCoroutine(UIUtility.SetHideUI(_AlertProgress.gameObject, true, true));
  }

  public void Start(){
    _path_follower = GetComponent<PathFollower>();
    _movement_controller = GetComponent<MovementController>();

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _on_scene_changed;
    _game_handler.SceneRemovingEvent += _on_scene_removing;


    if(_SoundReceiver != null)
      _SoundReceiver.SoundReceivedEvent += _patrol_soundalerted;

    _AlertingObject.AlertObjectEnterEvent += _on_alerted_enter;
    _AlertingObject.AlertObjectExitedEvent += _on_alerted_exit;

    if(_game_handler.SceneInitialized)
      _on_scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());

    StartCoroutine(_start_co_func());
  }


  public void Update(){
    // while the Game Over delay timer is still progressing, update the "Alerted" UI
    if(_gameover_delay < _GameOverTimeout)
      _update_alert_bar();
  }

  public void FixedUpdate(){
    // handles the Game Over delay timer
    if(_gameover_triggers.Count > 0){
      DEBUGModeUtils.Log("Stop delay refreshed");
      _stop_delay = _TemporaryStopDelay;

      if(_gameover_delay > 0){
        _gameover_delay -= Time.fixedDeltaTime;
        if(_gameover_delay < 0)
          _game_handler.TriggerPlayerSpotted();
      }
    }
    else{
      if(_gameover_delay < _GameOverTimeout){
        _stop_delay = _TemporaryStopDelay;
        _gameover_delay += Time.fixedDeltaTime;
        if(_gameover_delay >= _GameOverTimeout && _AlertProgress != null){
          StartCoroutine(UIUtility.SetHideUI(_AlertProgress.gameObject, true));
        }
      }
    }
  }


  /// <summary>
  /// Start the patrol action. It will skip the starting process if the object already started and not yet stopped.
  /// </summary>
  /// <returns>Is the starting process successful or not</returns>
  public bool StartPatrol(){
    if(IsPatrolling())  
      return false;

    _patrol_coroutine = StartCoroutine(_patrolling_function());
    return true;
  }

  /// <summary>
  /// Stop the patrol action. It will skip the stopping process if the object already stopped.
  /// </summary>
  /// <returns>Is the stopping process successful or not</returns>
  public bool StopPatrol(){
    if(!IsPatrolling())
      return false;

    if(!_path_follower.CancelMoving())
      return false;

    StopCoroutine(_patrol_coroutine);
    _patrol_coroutine = null;

    return true;
  }


  /// <summary>
  /// Check if the patrol action currently still running or not.
  /// </summary>
  /// <returns>Is still patrolling or not</returns>
  public bool IsPatrolling(){
    return _patrol_coroutine != null; 
  }


  /// <summary>
  /// Function to catch Unity's "Object Enabled" event.
  /// </summary>
  public void OnEnable(){
    if(_game_handler == null || !_game_handler.SceneInitialized)
      return;

    _restart_func();
  }


  /// <summary>
  /// Set patrolling data for the patrol system to be able to start.
  /// </summary>
  /// <param name="data">The patrolling data</param>
  public void SetInitData(InitData data){
    _init_data = data;
  }
}


/// <summary>
/// Interface class used for defining actions for patrolling.
/// </summary>
public interface PatrolAction{
  /// <summary>
  /// Getting the inherited class type.
  /// </summary>
  /// <returns>The class type</returns>
  public Type GetPatrolType();

  /// <summary>
  /// Coroutine function that will control <see cref="PatrolBehaviour"/> based on the inheriting class.
  /// </summary>
  /// <param name="behaviour"></param>
  /// <returns></returns>
  public IEnumerator DoAction(PatrolBehaviour behaviour);
}