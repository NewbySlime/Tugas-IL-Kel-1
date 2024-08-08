using System.Collections;
using UnityEngine;


/// <summary>
/// Extended <see cref="SequenceHandlerVS"/> component for utilizing Unity's "Enabling" event to trigger sequence stored in the base class.
/// 
/// This class uses autoload(s);
/// - <see cref="GameHandler"/> for game events and such.
/// </summary>
public class ActiveStateTriggerSequence: SequenceHandlerVS{
  [SerializeField]
  private bool _TriggerOnlyOnce;


  private GameHandler _game_handler;

  private bool _already_triggered = false;


  private IEnumerator _trigger_sequence_co_func(){
    yield return new WaitUntil(() => SequenceInitializeDataSet);
    DEBUGModeUtils.Log("triggering sequence");

    if(IsTriggering())
      yield break;

    StartTriggerAsync();
  }

  // Bridging function to _trigger_sequence_co_func 
  private void _trigger_sequence(){
    if(!gameObject.activeInHierarchy || _game_handler == null || !_game_handler.SceneInitialized || (_TriggerOnlyOnce && _already_triggered))
      return;

    DEBUGModeUtils.Log(string.Format("triggering bool {0} {1} {2} {3}", !gameObject.activeInHierarchy, _game_handler == null, !_game_handler.SceneInitialized,(_TriggerOnlyOnce && _already_triggered)));
    _already_triggered = true;
    StartCoroutine(_trigger_sequence_co_func());
  }


  // Coroutine for "OnEnable" event but it will wait until next update for safe triggering.
  private IEnumerator _on_enable(){
    yield return null;
    yield return new WaitForEndOfFrame(); 

    _trigger_sequence();
  }


  private void _game_handler_scene_changed(string scene_id, GameHandler.GameContext context){
    _trigger_sequence();
  }

  private void _game_handler_scene_removed(){
    _game_handler.SceneChangedFinishedEvent -= _game_handler_scene_changed;
    _game_handler.SceneRemovingEvent -= _game_handler_scene_removed;
  }


  ~ActiveStateTriggerSequence(){
    _game_handler_scene_removed();
  }


  public new void Start(){
    base.Start();

    _game_handler = FindAnyObjectByType<GameHandler>();
    if(_game_handler == null){
      Debug.LogError("Cannot find GameHandler.");
      throw new MissingReferenceException();
    }

    _game_handler.SceneChangedFinishedEvent += _game_handler_scene_changed;
    _game_handler.SceneRemovingEvent += _game_handler_scene_removed;

    if(_game_handler.SceneInitialized)
      _game_handler_scene_changed(_game_handler.GetCurrentSceneID(), _game_handler.GetCurrentSceneContext());
  }


  /// <summary>
  /// Function to catch Unity's "Enabled" event.
  /// </summary>
  public void OnEnable(){
    StartCoroutine(_on_enable());
  }
}