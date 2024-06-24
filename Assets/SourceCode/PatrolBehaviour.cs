using PatrolActions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[RequireComponent(typeof(PathFollower))]
[RequireComponent(typeof(MovementController))]
public class PatrolBehaviour: MonoBehaviour{
  [SerializeField]
  private float _TemporaryStopDelay = 2f;

  [SerializeField]
  private float _GameOverTimeout = 5f;

  [SerializeField]
  private AlertTrigger _AlertingObject;

  [SerializeField]
  private float __PathUpdateInterval = 0.3f;
  internal float _PathUpdateInterval{get => __PathUpdateInterval;}

  // NOTE: nullable
  [SerializeField]
  private BaseProgressUI _AlertProgress;

  // NOTE: nullable
  [SerializeField]
  private SoundAlertReceiver _SoundReceiver = null;


  public class InitData{
    public List<PatrolAction> PatrolSet = new List<PatrolAction>();
  }


  private static HashSet<Type> _allowed_skip_type = new HashSet<Type>{
    typeof(PatrolActions.WaitForSeconds),
    typeof(PatrolActions.LookAtDirection)
  };

  private GameHandler _game_handler;

  private PathFollower _path_follower;
  internal PathFollower _PathFollower{get => _path_follower;}

  private MovementController _movement_controller;
  internal MovementController _MovementController{get => _movement_controller;}

  private InitData _init_data;

  private int _current_patrol_idx = 0;
  private Coroutine _patrol_coroutine = null;
  private Coroutine _patrol_tmpstop = null;
  private Coroutine _patrol_forcestopper = null;

  private float _stop_delay = 0f;
  private float _gameover_delay = 0f;

  private HashSet<GameObject> _gameover_triggers = new HashSet<GameObject>();


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

  // set _stop_delay first
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
    // cek kalau yang masuk player
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
    // cek kalau yang keluar player
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
  }

  public void FixedUpdate(){
    if(_gameover_triggers.Count > 0){
      DEBUGModeUtils.Log("Stop delay refreshed");
      _stop_delay = _TemporaryStopDelay;

      if(_gameover_delay > 0){
        _gameover_delay -= Time.fixedDeltaTime;
        if(_gameover_delay < 0)
          _game_handler.TriggerPlayerSpotted();
      }

      _update_alert_bar();
    }
    else{
      if(_gameover_delay < _GameOverTimeout){
        _stop_delay = _TemporaryStopDelay;
        _gameover_delay += Time.fixedDeltaTime;
        if(_gameover_delay >= _GameOverTimeout && _AlertProgress != null){
          StartCoroutine(UIUtility.SetHideUI(_AlertProgress.gameObject, true));
        }

        _update_alert_bar();
      }
    }
  }


  public bool StartPatrol(){
    if(IsPatrolling())  
      return false;

    _patrol_coroutine = StartCoroutine(_patrolling_function());
    return true;
  }

  public bool StopPatrol(){
    if(!IsPatrolling())
      return false;

    if(!_path_follower.CancelMoving())
      return false;

    StopCoroutine(_patrol_coroutine);
    _patrol_coroutine = null;

    return true;
  }


  public bool IsPatrolling(){
    return _patrol_coroutine != null; 
  }

  public bool CancelPatrolling(){
    if(!IsPatrolling())
      return false;

    if(_patrol_coroutine != null)
      StopCoroutine(_patrol_coroutine);
    _patrol_coroutine = null;

    return true;
  }


  public void OnEnable(){
    if(_game_handler == null || !_game_handler.SceneInitialized)
      return;

    _restart_func();
  }


  public void SetInitData(InitData data){
    _init_data = data;
  }
}


public interface PatrolAction{
  public Type GetPatrolType();
  public IEnumerator DoAction(PatrolBehaviour behaviour);
}