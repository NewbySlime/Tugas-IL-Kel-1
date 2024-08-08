using Pathfinding;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Reflection;



[RequireComponent(typeof(PathFollower))]
/// <summary>
/// Behavoiur class that will make the object randomly walk in any direction to make the object/NPC feels more alive.
/// 
/// This class uses following component(s);
/// - <see cref="PathFollower"/> component for pathfinding and AI movement.
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for waiting a scene to finish (when changing scene).
/// </summary>
public class NPCRandomBehaviour: MonoBehaviour{
  /// <summary>
  /// Max iteration to calculate in one pathfinding update.
  /// </summary>
  public const float MaxRandomPositionIteration = 10;

  [SerializeField]
  private float _MaxRandomPositionDistance = 5;
  [SerializeField]
  private float _MinRandomPositionDistance = -5;

  [SerializeField]
  private float _MaxRandomChangePosTime = 10;
  [SerializeField]
  private float _MinRandomChangePosTime = 4;


  private PathFollower _path_follower;

  private GameHandler _game_handler;
  
  
  private float _idle_timer = 0;

  private bool _cannot_find_path = false;
  private bool _is_path_searching = false; 


  /// <summary>
  /// Flag if the object has been initialized or not.
  /// </summary>
  public bool IsInitialized{private set; get;} = false;


  private void _on_path_error(){
    _cannot_find_path = true;
    _is_path_searching = false;
  }

  private void _on_path_found(){
    _cannot_find_path = false;
    _is_path_searching = false;
  }

  // pathfinding update
  private IEnumerator _start_random_path(){
    yield return null;
    yield return new WaitForEndOfFrame();

    for(int i = 0; i < MaxRandomPositionIteration; i++){
      Vector2 _random_new_pos = (Vector2)transform.position + new Vector2(MathExt.Range(_MinRandomPositionDistance, _MaxRandomPositionDistance, Random.value), 0);

      _cannot_find_path = false;
      _is_path_searching = true;

      _path_follower.FollowPathAsync(_random_new_pos);
      yield return new WaitUntil(() => !_is_path_searching);

      if(!_cannot_find_path)
        break;
    }

    if(_cannot_find_path)
      Debug.LogWarning("Cannot find path for NPC, is it stuck?");

    yield return new WaitUntil(() => !_path_follower.IsMoving());
    _start_random_wait();
  }

  // pathfinding cooldown/delay
  private void _start_random_wait(){
    _idle_timer = Random.Range(_MinRandomChangePosTime, _MaxRandomChangePosTime);
  }


  // restart this object
  private void _restart_func(){
    //_start_random_wait();
    StartCoroutine(_start_random_path());
  }


  private void _on_scene_changed(string scene_id, GameHandler.GameContext context){
    if(!gameObject.activeInHierarchy)
      return;

    _restart_func();
  }

  private void _on_scene_removed(){
    _game_handler.SceneChangedFinishedEvent -= _on_scene_changed;
    _game_handler.SceneRemovingEvent -= _on_scene_removed;
  }


  /// <summary>
  /// Function to catch Unity's "Object Destroy" event.
  /// </summary>
  public void OnDestroy(){
    if(!IsInitialized)
      return;

    _on_scene_removed();
  }


  public void Start(){
    _path_follower = GetComponent<PathFollower>();
    if(_path_follower == null){
      Debug.LogError("Cannot find PathFollower.");
      throw new MissingComponentException();
    }

    _path_follower.PathFoundEvent += _on_path_found;
    _path_follower.PathErrorEvent += _on_path_error;

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingComponentException();
    }

    _game_handler.SceneChangedFinishedEvent += _on_scene_changed;
    _game_handler.SceneRemovingEvent += _on_scene_removed;

    IsInitialized = true;
    if(_game_handler.SceneInitialized)
      _on_scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }


  public void FixedUpdate(){
    // processing timer for pathfinding cooldown
    if(_idle_timer > 0){
      _idle_timer -= Time.fixedDeltaTime;
      if(_idle_timer <= 0)
        StartCoroutine(_start_random_path());
    }
  }


  /// <summary>
  /// Restart this behaviour and also process a new random path.
  /// </summary>
  public void RestartBehaviour(){
    _path_follower.CancelMoving();
    _restart_func();
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
  /// Function to catch Unity's "Object Disabled" event.
  /// </summary>
  public void OnDisable(){
    _path_follower.CancelMoving();
  }
}